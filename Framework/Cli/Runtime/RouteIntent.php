<?php

namespace Framework\Cli\Runtime;

final class RouteIntent
{
    private string $app;
    private string $page;
    private array $args;

    public function __construct(string $app, string $page, array $args = [])
    {
        $this->app = $app;
        $this->page = $page;
        $this->args = $args;
    }

    public function app(): string
    {
        return $this->app;
    }

    public function page(): string
    {
        return $this->page;
    }

    public function args(): array
    {
        return $this->args;
    }
}
