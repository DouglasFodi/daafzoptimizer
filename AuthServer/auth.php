<?php
declare(strict_types=1);

header('Content-Type: application/json; charset=utf-8');
header('Cache-Control: no-store, no-cache, must-revalidate, max-age=0');
header('Pragma: no-cache');

function reply(int $status, bool $ok, string $message): never {
    http_response_code($status);
    echo json_encode(['ok' => $ok, 'message' => $message], JSON_UNESCAPED_UNICODE | JSON_UNESCAPED_SLASHES);
    exit;
}

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    reply(405, false, 'Método não permitido.');
}

// Preferência 1: variável de ambiente no servidor.
$passwordHash = getenv('REGOPTIMIZER_PASSWORD_HASH') ?: '';

// Preferência 2: arquivo local NÃO versionado.
$secretFile = __DIR__ . '/secret.php';
if ($passwordHash === '' && is_file($secretFile)) {
    $secret = require $secretFile;
    if (is_array($secret) && isset($secret['password_hash'])) {
        $passwordHash = (string)$secret['password_hash'];
    }
}

if ($passwordHash === '' || str_contains($passwordHash, 'COLE_AQUI')) {
    error_log('RegOptimizer auth: password hash not configured.');
    reply(503, false, 'Servidor de autenticação não configurado.');
}

$raw = file_get_contents('php://input');
$data = json_decode($raw ?: '', true);
if (!is_array($data)) {
    reply(400, false, 'Requisição inválida.');
}

$password = isset($data['password']) ? (string)$data['password'] : '';
$application = isset($data['application']) ? (string)$data['application'] : '';

if ($application !== 'RegOptimizer' || $password === '') {
    reply(401, false, 'Acesso recusado.');
}

// Limite simples por IP: 5 falhas em 10 minutos.
$ip = $_SERVER['REMOTE_ADDR'] ?? 'unknown';
$bucket = sys_get_temp_dir() . '/regoptimizer-auth-' . hash('sha256', $ip) . '.json';
$now = time();
$windowSeconds = 600;
$maxFailures = 5;
$state = ['start' => $now, 'failures' => 0];

if (is_file($bucket)) {
    $loaded = json_decode((string)file_get_contents($bucket), true);
    if (is_array($loaded) && isset($loaded['start'], $loaded['failures'])) {
        $state = ['start' => (int)$loaded['start'], 'failures' => (int)$loaded['failures']];
    }
}

if (($now - $state['start']) > $windowSeconds) {
    $state = ['start' => $now, 'failures' => 0];
}

if ($state['failures'] >= $maxFailures) {
    reply(429, false, 'Muitas tentativas. Aguarde alguns minutos.');
}

if (!password_verify($password, $passwordHash)) {
    $state['failures']++;
    @file_put_contents($bucket, json_encode($state), LOCK_EX);
    usleep(350000); // desacelera brute force básico
    reply(401, false, 'Senha inválida.');
}

@unlink($bucket);
reply(200, true, 'Acesso autorizado.');
