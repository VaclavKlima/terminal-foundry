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
$kernel->run($_SERVER['argv'] ?? []);
