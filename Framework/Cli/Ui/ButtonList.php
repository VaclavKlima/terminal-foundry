<?php

namespace Framework\Cli\Ui;

final class ButtonList implements Element
{
    private string $title;
    private array $items;

    public function __construct(string $title, array $items)
    {
        $this->title = $title;
        $this->items = $items;
    }

    public function render(RenderContext $context): string
    {
        if ($this->items === []) {
            return '';
        }

        $palette = $context->palette();
        $primary = $palette['primary'] ?? '';
        $muted = $palette['muted'] ?? '';
        $reset = $palette['reset'] ?? '';

        $lines = [$primary . $this->title . $reset];
        foreach ($this->items as $item) {
            $key = $item['key'] ?? '';
            $label = $item['label'] ?? '';
            $active = (bool) ($item['active'] ?? false);
            $prefix = $active ? '>' : ' ';
            $color = $active ? $primary : $muted;
            $lines[] = sprintf('%s[%s] %s%s', $prefix, $key, $color, $label) . $reset;
        }

        return implode(PHP_EOL, $lines);
    }
}
