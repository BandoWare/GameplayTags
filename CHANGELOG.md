# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

### Added

- Added warning log when a tag cannot be found in `GameplayTagManager.RequestTag`.
- Added interfece `IGameplayTagCountContainer`.

### Fixed

- Fixed debugger display in `GameplayTagCountContainer`.
- Fixed bug in `GameplayTagContainer.HasAll` that was returning false when the comparator container had only one tag.
- Fixed bug where root game tags had an empty `HierarchyTags`.

### Changed

- Unchecked option **Auto Referenced** in the Editor assembly.

## [0.1.0-beta.3]

### Added

- Added `GetParentTags` and `GetChildTags` to `IGameplayTagContainer`.

### Fixed

- Fixed `GameplayTagContainerUtility.HasAll{Exact}` logic.
- Fixed a bug in `GameplayTagContainer.FillImplicitTags`: Corrected the index check to use the last element in the list properly.

## [0.1.0-beta.2] - 2023-06-06

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