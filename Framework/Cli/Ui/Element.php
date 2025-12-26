<?php

namespace Framework\Cli\Ui;

interface Element
{
    public function render(RenderContext $context): string;
}
