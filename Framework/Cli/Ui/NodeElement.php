<?php

namespace Framework\Cli\Ui;

interface NodeElement
{
    public function toNode(RenderContext $context): array;
}
