<?php

namespace Framework\Cli\Runtime;

final class RouteSelection
{
    private Application $app;
    private PageDefinition $page;
    private array $args;

    public function __construct(Application $app, PageDefinition $page, array $args = [])
    {
        $this->app = $app;
        $this->page = $page;
        $this->args = $args;
    }

    public function app(): Application
    {
        return $this->app;
    }

    public function page(): PageDefinition
    {
        return $this->page;
    }

    public function args(): array
    {
        return $this->args;
    }
}
