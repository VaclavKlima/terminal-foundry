<?php

namespace Framework\Cli\Runtime;

final class FormState
{
    private array $values = [];

    public function get(string $scope, string $key, $default = '')
    {
        $full = $scope . $key;
        return array_key_exists($full, $this->values) ? $this->values[$full] : $default;
    }

    public function set(string $scope, string $key, $value): void
    {
        $this->values[$scope . $key] = $value;
    }
}
