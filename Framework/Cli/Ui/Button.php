<?php

namespace Framework\Cli\Ui;

final class Button
{
    use HandlesOnClick;

    private string $label;
    private ?string $hint;
    private bool $active;
    private ?array $actionArgs;

    private function __construct(string $label, ?string $hint, bool $active, ?array $actionArgs)
    {
        $this->label = $label;
        $this->hint = $hint;
        $this->active = $active;
        $this->actionArgs = $actionArgs;
    }

    public static function make(string $label): self
    {
        return new self($label, null, false, null);
    }

    public function hint(string $hint): self
    {
        $this->hint = $hint;
        return $this;
    }

    public function active(bool $active = true): self
    {
        $this->active = $active;
        return $this;
    }

    public function route(string $app, string $page, array $args = []): self
    {
        $this->onClick(static function () use ($app, $page, $args) {
            return \Framework\Cli\Runtime\Router::to($app, $page, $args);
        });
        return $this;
    }

    public function label(): string
    {
        return $this->label;
    }

    public function hintText(): ?string
    {
        return $this->hint;
    }

    public function isActive(): bool
    {
        return $this->active;
    }

    public function actionArgs(): ?array
    {
        return $this->actionArgs;
    }

    protected function clearActionArgs(): void
    {
        $this->actionArgs = null;
    }
}
