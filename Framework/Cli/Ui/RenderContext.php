<?php

namespace Framework\Cli\Ui;

final class RenderContext
{
    private int $columns;
    private string $colorScheme;
    private array $palette;
    private RenderState $state;

    public function __construct(int $columns, string $colorScheme, array $palette, RenderState $state)
    {
        $this->columns = max(1, $columns);
        $this->colorScheme = self::normalizeScheme($colorScheme);
        $this->palette = $palette;
        $this->state = $state;
    }

    public static function fromEnvironment(?RenderState $state = null): self
    {
        $columns = 80;
        $envColumns = getenv('COLUMNS');
        if ($envColumns !== false && is_numeric($envColumns)) {
            $columns = (int) $envColumns;
        }

        $scheme = getenv('APP_COLOR_SCHEME');
        if ($scheme === false || $scheme === '') {
            $scheme = 'dark';
        }

        $palette = [
            'primary' => "\x1b[38;5;75m",
            'muted' => "\x1b[38;5;245m",
            'reset' => "\x1b[0m",
        ];

        return new self($columns, $scheme, $palette, $state ?? new RenderState());
    }

    public function withOverrides(?int $columns, ?string $colorScheme, ?array $palette): self
    {
        return new self(
            $columns ?? $this->columns,
            $colorScheme ?? $this->colorScheme,
            $palette ?? $this->palette,
            $this->state
        );
    }

    public function columns(): int
    {
        return $this->columns;
    }

    public function colorScheme(): string
    {
        return $this->colorScheme;
    }

    public function palette(): array
    {
        return $this->palette;
    }

    public function addAction(array $action): void
    {
        $this->state->addAction($action);
    }

    public function actions(): array
    {
        return $this->state->actions();
    }

    public function nextNodeId(string $prefix): string
    {
        return $this->state->nextNodeId($prefix);
    }

    private static function normalizeScheme(string $scheme): string
    {
        $normalized = strtolower(trim($scheme));
        if ($normalized === 'light' || $normalized === 'dark' || $normalized === 'auto') {
            return $normalized;
        }

        throw new \InvalidArgumentException("Unsupported color scheme: {$scheme}");
    }
}
