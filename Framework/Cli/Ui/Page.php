<?php

namespace Framework\Cli\Ui;

final class Page implements Element, NodeElement
{
    private string $title;
    private array $sections = [];

    private function __construct(string $title)
    {
        $this->title = $title;
    }

    public static function make(string $title): self
    {
        return new self($title);
    }

    public function schema(array $sections): self
    {
        $this->sections = $sections;
        return $this;
    }

    public function section(string $title, callable $builder): self
    {
        $section = new Section($title);
        $builder($section);
        $this->sections[] = $section;

        return $this;
    }

    public function add(Element $element): self
    {
        $this->sections[] = $element;
        return $this;
    }

    public function render(RenderContext $context): string
    {
        $palette = $context->palette();
        $primary = $palette['primary'] ?? '';
        $reset = $palette['reset'] ?? '';

        $lines = [];
        $title = $this->title;
        $lines[] = $primary . $title . $reset;
        $lines[] = str_repeat('-', min($context->columns(), max(3, strlen($title))));

        foreach ($this->sections as $section) {
            $output = $section->render($context);
            if ($output !== '') {
                $lines[] = $output;
            }
        }

        return implode(PHP_EOL . PHP_EOL, $lines);
    }

    public function toNode(RenderContext $context): array
    {
        $children = [];
        foreach ($this->sections as $section) {
            $children[] = NodeBuilder::nodeFor($section, $context);
        }

        return [
            'type' => 'page',
            'title' => $this->title,
            'children' => $children,
        ];
    }
}
