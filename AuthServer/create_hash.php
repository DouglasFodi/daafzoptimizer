<?php
declare(strict_types=1);

if (PHP_SAPI !== 'cli') {
    http_response_code(404);
    exit;
}

$password = $argv[1] ?? '';
if ($password === '') {
    fwrite(STDERR, "Uso: php create_hash.php \"SUA-SENHA\"\n");
    exit(1);
}

echo password_hash($password, PASSWORD_DEFAULT) . PHP_EOL;
