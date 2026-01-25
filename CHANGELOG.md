# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0-alpha] - 2026-01-25

### Added
- Comprehensive unit testing framework with 400+ lines of test coverage
- Unit tests for all major logger functionality
- MockConsoleWriter for testing purposes
- StreamWriter-based console output for improved performance
- Message batching system for better performance
- Configurable MaxBatchSize parameter via Configure() method
- Thread-safe configuration with locking mechanism
- Constants and improved documentation for batching parameters

### Changed
- Switched from direct Console.Write to StreamWriter for better control
- Optimized flush behavior with manual flush control
- Improved MaxBatchSize validation and reasonable value limits

### Fixed
- Color swapping unit tests
- Thread-safety issues in Configure() method

## [0.1.6-alpha] - 2026-01-25

### Fixed
- Catches error during set cursor position (which occurs when putting the app in background)

## [0.1.5-alpha] - 2026-01-25

### Added
- Min log level option

## [0.1.4-alpha] - 2026-01-25

### Changed
- Upgraded message queue to use structs instead of records
- Implemented Disposable pattern
- Reorganized some files
- Enabled .NET 9 and 10 targeting

## [0.1.3-alpha] - 2026-01-25

### Fixed
- Wrong value returned as Queue length

## [0.1.2-alpha] - 2026-01-25

### Added
- Bunch of overloads and XML docs

## [0.1.1-alpha] - 2026-01-25

### Changed
- Bumped version up

## [0.1.0-alpha] - 2026-01-25

### Added
- Initial release
- Basic console logging functionality
- Support for updating previous message
- Multiple log levels (Info, Warning, Error, Debug)
- Singleton pattern implementation
