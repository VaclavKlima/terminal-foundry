<?php

namespace Framework\Cli\Runtime;
use Framework\Cli\Ui\Element;

abstract class PageDefinition
{
    abstract public function name(): string;

    abstract public function build(array $args, callable $get, callable $set): Element;

    final public function buildElement(array $args, callable $get, callable $set): Element
    {
        return $this->build($args, $get, $set);
    }
}
