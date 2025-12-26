<?php

namespace Framework\Cli\Runtime;

final class Router
{
    public function resolve(array $argv, Registry $registry): RouteSelection
    {
        $appName = $argv[1] ?? '';
        $pageName = $argv[2] ?? '';
        $args = array_slice($argv, 3);

        $app = $appName !== '' ? $registry->app($appName) : $registry->defaultApp();
        $page = $pageName !== '' ? $app->page($pageName) : $app->defaultPage();

        return new RouteSelection($app, $page, $args);
    }
}
