<?php

namespace Framework\Cli\Runtime;

final class ActionRegistry
{
    private static array $handlers = [];

    public static function register(callable $handler): string
    {
        $id = bin2hex(random_bytes(8));
        self::$handlers[$id] = $handler;
        return $id;
    }

    public static function invoke(string $id, array $args = [])
    {
        if (!isset(self::$handlers[$id])) {
            throw new \RuntimeException("Unknown action id: {$id}");
        }

        $handler = self::$handlers[$id];
        $reflection = new \ReflectionFunction(\Closure::fromCallable($handler));
        $params = $reflection->getParameters();
        $maxArgs = $reflection->isVariadic() ? count($args) : count($params);
        $callArgs = array_slice($args, 0, $maxArgs);

        return call_user_func_array($handler, $callArgs);
    }
}
