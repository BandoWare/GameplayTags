# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- Fixed GameplayTags not loading from StreamingAssets on Android (APK/AAB)
- Fixed GameplayTags not loading from StreamingAssets on WebGL

## [0.1.0-beta.8] - 2025-09-22

### Fixed

- Removed inappropriate `ValidateIsNone` usage from `GameplayTag` definition property.

## [0.1.0-beta.7] - 2025-09-22

### Added

- Added `IReadOnlyGameplayTagContainer` and `IReadOnlyGameplayTagContainer` interfaces.
 -Added explicit validity mismatch checks in GameplayTag comparisons to ensure correct equality behavior.

### Fixed 

- Fixed conflicting conditions in GameplayTagPropertyDrawer by changing if to else if and removing redundant invalid-tag check.

### Removed

- Removed `GameplayTagHierarchicalContanier`.

## [0.1.0-beta.6]

### Fixed

- Fixed a bug where opening the "Add New Tag" panel caused the tag TreeView to malfunction.

### Changed

- Added a bottom margin to GameplayTagContainer for better readability

## [0.1.0-beta.5]

### Added

- Added `GameplayTagContainerPool`.
- Added `AllGameplayTags` source generator.
- Added `GameplayTagContainer.GetDiffExplicitTags`.
- Added support for defining tags in JSON files inside `ProjectSettings/GameplayTags/`, in addition to the existing attribute-based registration.
- Added `GameplayTag.IsValid` property: indicates whether a tag is valid, making it easier to identify tags that were deleted, renamed, or referenced in code but no longer exist.

### Fixed

- Fixed `GameplayTagContainer.Clone()` implementation. (#1)
  - **Note:** Previous fix in this area was incorrect. This has now been properly addressed.

### Changed

 `AllGameplayTags` is now obsolete.
- `GameplayTagManager.RequestTag` will now return `GameplayTag.None` when it receives a null or empty string.
- `GamplayTagFlags.HideInEditor` is now obsolete and doesn't affect if the tag is visble or not in the Editor. Now, every tag that is child of "Test" tag won't be visible in the editor.
- `GameplayTagUtility` is now public.
- Enhanced the tag selection menu with multiple improvements, including the ability to add and remove tags directly from the Editor.

## [0.1.0-beta.4]

### Added

- Added `GameplayTagHierarchicalContainer`.
- Added a message in the GameplayTagContainer property drawer when in multi-edit mode and the container values are different.
- Added warning log when a tag cannot be found in `GameplayTagManager.RequestTag`.
- Added interfece `IGameplayTagCountContainer`.

### Fixed

- Fixed debugger display in `GameplayTagCountContainer`.
- Fixed bug in `GameplayTagContainer.HasAll` that was returning false when the comparator container had only one tag.
- Fixed bug where root game tags had an empty `HierarchyTags`.
- Fixed `GameplayTagContainer.Clone()` implementation. (#1)
- Fixed `HasAny(Exact)` returning true when it should return false. (#2)
- Fixed `GetExplicit(Child)Tags` implementation. (#3)

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