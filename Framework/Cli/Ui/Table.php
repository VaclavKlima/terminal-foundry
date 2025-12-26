<?php

namespace Framework\Cli\Ui;

final class Table implements Element, NodeElement
{
    private array $headers;
    private array $rows;

    private function __construct(array $headers, array $rows)
    {
        $this->headers = $headers;
        $this->rows = $rows;
    }

    public static function make(array $headers, array $rows = []): self
    {
        return new self($headers, $rows);
    }

    public function rows(array $rows): self
    {
        $this->rows = $rows;
        return $this;
    }

    public function row(array $row): self
    {
        $this->rows[] = $row;
        return $this;
    }

    public function render(RenderContext $context): string
    {
        $rows = $this->rows;
        $headers = $this->headers;
        $widths = [];

        foreach ($headers as $index => $header) {
            $widths[$index] = strlen((string) $header);
        }

        foreach ($rows as $row) {
            foreach ($row as $index => $cell) {
                $length = strlen((string) $cell);
                if (!isset($widths[$index]) || $length > $widths[$index]) {
                    $widths[$index] = $length;
                }
            }
        }

        $lines = [];
        if ($headers !== []) {
            $lines[] = $this->renderRow($headers, $widths);
            $lines[] = $this->renderDivider($widths);
        }

        foreach ($rows as $row) {
            $lines[] = $this->renderRow($row, $widths);
        }

        return implode(PHP_EOL, $lines);
    }

    public function toNode(RenderContext $context): array
    {
        return [
            'type' => 'table',
            'headers' => array_values($this->headers),
            'rows' => array_values($this->rows),
        ];
    }

    private function renderRow(array $row, array $widths): string
    {
        $cells = [];
        foreach ($widths as $index => $width) {
            $value = isset($row[$index]) ? (string) $row[$index] : '';
            $cells[] = str_pad($value, $width, ' ');
        }

        return '| ' . implode(' | ', $cells) . ' |';
    }

    private function renderDivider(array $widths): string
    {
        $cells = [];
        foreach ($widths as $width) {
            $cells[] = str_repeat('-', $width);
        }

        return '|-' . implode('-|-', $cells) . '-|';
    }
}
