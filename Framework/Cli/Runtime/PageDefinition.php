<?php

namespace Framework\Cli\Runtime;
use Framework\Cli\Ui\Element;

abstract class PageDefinition
{
    abstract public function name(): string;

    abstract public function build(array $args): Element;

    final public function buildElement(array $args): Element
    {
        return $this->build($args);
    }
}
