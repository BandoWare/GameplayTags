# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- Fixed `GameplayTagContainerUtility.HasAll{Exact}` logic.

## [0.0.1-beta.2] - 2023-06-06

### Added

- Improved performance of `GameplayTagContainer` when filling parent tags for remaining tags after tag removal.
- Enhanced debugging by adding a debugger proxy type to `GameplayTagCountContainer`.
- Implemented `IEnumerable<GameplayTag>` for `GameplayTagCountContainer` and `GameplayTagContainer`.

### Fixed

- Corrected logic for `GameplayTagContainer.AddIntersection`.

### Changed

- Changed access modifier of `GameplayTagContainer.AddIntersection` to `private`.

### Removed

- Removed the `GameObjectGameplayTagContainer.m_Tag` field, which was accidentally left in the script.

## [0.1.0-beta.1] - 2023-05-21

### Initial Release

- First beta release of the package.