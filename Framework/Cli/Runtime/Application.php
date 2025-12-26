<?php

namespace Framework\Cli\Runtime;

final class Application
{
    private string $name;
    private array $uiOptions;
    private array $pages = [];
    private ?string $defaultPage = null;

    public function __construct(string $name, array $uiOptions = [])
    {
        $this->name = $name;
        $this->uiOptions = $uiOptions;
    }

    public function name(): string
    {
        return $this->name;
    }

    public function uiOptions(): array
    {
        return $this->uiOptions;
    }

    public function addPage(string $pageClass, bool $isDefault = false): void
    {
        if (!class_exists($pageClass)) {
            throw new \RuntimeException("Page class not found: {$pageClass}");
        }

        $page = new $pageClass();
        if (!$page instanceof PageDefinition) {
            throw new \RuntimeException("Page class {$pageClass} must extend PageDefinition.");
        }

        $this->pages[$page->name()] = $pageClass;

        if ($isDefault || $this->defaultPage === null) {
            $this->defaultPage = $page->name();
        }
    }

    public function page(string $name): PageDefinition
    {
        if (!isset($this->pages[$name])) {
            throw new \RuntimeException("Unknown page: {$name}");
        }

        $class = $this->pages[$name];
        return new $class();
    }

    public function defaultPage(): PageDefinition
    {
        if ($this->defaultPage === null) {
            throw new \RuntimeException("No default page configured for app {$this->name}");
        }

        $class = $this->pages[$this->defaultPage];
        return new $class();
    }
}
