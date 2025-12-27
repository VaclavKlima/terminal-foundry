<?php

namespace Framework\Cli\Runtime;

final class Log
{
    public static function info(string $message): void
    {
        self::write('INFO', $message);
    }

    public static function warning(string $message): void
    {
        self::write('WARNING', $message);
    }

    public static function error(string $message): void
    {
        self::write('ERROR', $message);
    }

    private static function write(string $level, string $message): void
    {
        $line = sprintf('[%s] [%s] %s', date('Y-m-d H:i:s'), $level, $message);
        error_log($line);
    }
}
