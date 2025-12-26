<?php

namespace App\Apps;

use Framework\Cli\Runtime\App;
use Framework\Cli\Runtime\AppDefinition;
use App\Pages\AboutPage;
use App\Pages\HomePage;
use Framework\Cli\Runtime\Application;

#[App('main')]
final class MainApp extends AppDefinition
{
    public function name(): string
    {
        return 'main';
    }

    public function uiOptions(): array
    {
        return [
            'columns' => 64,
            'colorScheme' => 'dark',
        ];
    }

    protected function registerPages(Application $app): void
    {
        $app->addPage(HomePage::class, true);
        $app->addPage(AboutPage::class);
    }
}
