<?php

namespace App\Apps;

use Framework\Cli\Runtime\App;
use Framework\Cli\Runtime\AppDefinition;
use App\Pages\StatusPage;
use Framework\Cli\Runtime\Application;

#[App('tools')]
final class ToolsApp extends AppDefinition
{
    public function name(): string
    {
        return 'tools';
    }

    public function uiOptions(): array
    {
        return [
            'columns' => 72,
            'colorScheme' => 'dark',
        ];
    }

    protected function registerPages(Application $app): void
    {
        $app->addPage(StatusPage::class, true);
    }
}
