<?php

namespace Framework\Cli\Runtime;

#[\Attribute(\Attribute::TARGET_CLASS)]
final class App
{
    private string $name;

    public function __construct(string $name)
    {
        $this->name = $name;
    }

    public function name(): string
    {
        return $this->name;
    }
}
