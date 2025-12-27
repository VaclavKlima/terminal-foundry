<?php

require_once 'vendor/autoload.php';

spl_autoload_register(function (string $class): void {
    $roots = [
        'App\\' => __DIR__ . '/App/',
        'Framework\\' => __DIR__ . '/Framework/',
    ];

    foreach ($roots as $prefix => $baseDir) {
        $prefixLength = strlen($prefix);
        if (strncmp($prefix, $class, $prefixLength) !== 0) {
            continue;
        }

        $relativeClass = substr($class, $prefixLength);
        $file = $baseDir . str_replace('\\', '/', $relativeClass) . '.php';
        if (is_file($file)) {
            require $file;
        }

        return;
    }
});

use App\Kernel;

$kernel = new Kernel();
$argv = $_SERVER['argv'] ?? [];
if (in_array('--ui-worker', $argv, true)) {
    putenv('APP_WORKER=1');
    $script = $argv[0] ?? 'index.php';
    while (($line = fgets(STDIN)) !== false) {
        $trimmed = trim($line);
        if ($trimmed === '') {
            continue;
        }

        $payload = json_decode($trimmed, true);
        if (!is_array($payload)) {
            continue;
        }

        $command = $payload['command'] ?? $payload['Command'] ?? null;
        if ($command === 'exit') {
            break;
        }

        $args = [];
        if (isset($payload['args']) && is_array($payload['args'])) {
            $args = $payload['args'];
        } elseif (isset($payload['Args']) && is_array($payload['Args'])) {
            $args = $payload['Args'];
        }

        $kernel->run(array_merge([$script], $args, ['--ui-json']));
        fflush(STDOUT);
    }
    exit(0);
}

putenv('APP_WORKER=0');
$kernel->run($argv);
