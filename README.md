![RoadBuilderLogoLandscape](https://github.com/user-attachments/assets/a2e0b7d9-13d5-4198-9f6b-caa41b8eb7df)
# Road Builder
**In collaboration with Trejak and BadPeanut**

Create endless road combinations on the fly while you’re playing the game!
With a slick and easy-to-use interface, you’ll be able to create the exact road that you need on the spot.

## Features

* Edit and create any road, in real-time, without leaving your game.
* Roads are unique and automatically saved when saving your city.
* Each road has its own auto-generated, detailed thumbnail.
* Roads are generated in real time, guaranteeing future compatibility with updates to the game.
* Roads are included in your save-game, allowing you to share it with anyone who has Road Builder with ease.
* You can create asphalt roads, gravel roads, pedestrian roads, highways, all three types of tracks, and pedestrian paths.

## Custom Lanes Support

Road Builder supports custom lanes made in the asset editor or by other mods: different sizes, textures and functionalities, all usable in any road you build.
See [Making Custom Lanes](#making-custom-lanes) for the full guide.


# How to Use
## Creating New Roads
All roads start from an existing network. You have to place a segment to then edit it.
This partially dictates how the generated road looks and behaves, but you'll always be able to change those settings later.


## Using Custom Roads
When you create a road, it is generated as a whole new asset which you can find and use from the road toolbar or the Custom Roads List.
You can also use the Road Builder tool to 'Place More' of the custom roads you created.


## How to Edit a Custom Road
Once you have a custom road, you can 'Edit All Instances' with Road Builder to automatically update all of the segments of that road that you've placed on the map.
You can, however, choose to create a new road based on that existing one. This will instead only update the single segment that you selected.
To do that, simply choose the 'Use As Template' option when selecting a segment.


## Using the Road Builder Interface
### Road Builder Panel
This panel on the right contains a tab for all the Available Lanes that can be used on your custom road, as well as a tab for all your previously Created Roads.
Using the Available Lanes tab, you can browse different types of lanes you can drag and drop onto the Road Layout preview.


### Road Layout Preview
Located in the center of your toolbar, the Road Layout Preview shows all the current lanes that are applied to your custom road. You can click and drag them to rearrange them, hover over a lane to see and apply any options the lane has, or you can click and drag the lane down into the bin to delete it.
On the far right of the toolbar you can also see the buttons to delete the current road, or X out of the Road Builder Interface.
On the far left of the toolbar there is the option to use as a template, or pick another road to edit.


### The Road Properties Panel
The left panel contains the properties of the road you are editing.
The **Category** section dictates the type of road, which is the same as the 'base' that we mentioned earlier.
The category will generally change what lanes are allowed to be used in the road, it will also change the category where the road is placed in the asset toolbar.

The **Addons** section toggles some extra options available for the road, like having raised sidewalks, zoning, etc…


## Advanced User Mode
Found in the game options panel, under “Road Builder” the Advanced User Mode exposes more lanes available in the Road Builder Panel. Some of the lanes may work in unexpected or unsupported ways, so player discretion is advised.


# Making Custom Lanes
Any net section can become a lane in Road Builder. Road Builder finds lanes through a set of **Road Builder components** added to the section (or to a lane group), so there's nothing to register and no code to write.

Because these components belong to Road Builder, **players need Road Builder installed to load your asset**. Set Road Builder as a required item when you publish it on Paradox Mods.

## Quick start with the Road Builder Setup panel
The asset editor has a helper panel that adds and fills in the components for you, so you only deal with what your lane needs.

1. Click the **Road Builder Setup** button in the asset editor's bottom bar.
2. With nothing selected, the panel opens to **Getting started**, which explains how lanes and groups fit together and lets you:
   * **Create a lane** or **edge lane** by copying an existing net section.
   * **Create a lane group**.
3. With a net section selected in the inspector, pick its **lane type** (see below).
4. Under **Road types**, pick which kinds of roads the lane can be added to.
5. Under **Lane list** and **Appearance**, pick the icon shown in Road Builder's lane list and the **lane icon** shown above the lane in the lane bar.
6. Optionally add **road thumbnail** images, used when Road Builder draws thumbnails for roads that use your lane.
7. Save the asset as usual. The components are saved with it.

The panel shows a live preview of the lane as it appears in Road Builder, and lists problems as you go: **errors** stop the lane from working, **warnings** point at things that probably aren't what you want.
Everything the panel does can also be done by hand in the inspector, under **Add Component → RoadBuilder**.

Road Builder's own lanes and the game's sections can't be edited. Use **Make an editable copy** in the panel to start your own version of one.
To add versions to a lane group, open the group's setup and use **New variant**, which copies a section as a variant of that group.

## Lane types
| Type | What it is | Components |
|---|---|---|
| **Lane** | A lane that shows up on its own in Road Builder's lane list. | `RoadBuilderLaneInfo` |
| **Edge lane** | A sidewalk or shoulder. Edge lanes are kept on the outer edges of the road, and decide what is added beyond them, like curbs. | `RoadBuilderLaneInfo` + `RoadBuilderEdgeLaneInfo` |
| **Group variant** | One version of a lane group. The group is what appears in the list, and the player picks between its variants with options, like the width of a car lane. | `RoadBuilderLaneGroup` |
| **Lane group** | A `LaneGroupPrefab` that bundles variants under one lane with options. | `LaneGroupPrefab` (+ optionally `RoadBuilderLaneInfo`) |

Most custom lanes only need to be a **Lane** or an **Edge lane**. Lane groups are for lanes that come in several versions.

## Images
* **Lane icons** (front, back, decoration icons, option buttons) can be any image. Use **Browse** in the panel to pick or import one, the same way as other icons in the asset editor.
* **Road thumbnail images** must be SVGs containing two rectangles with the ids `Position` and `Extents`, which tell Road Builder where to place the image in the road's thumbnail. Look at Road Builder's own `Thumb_*.svg` files for examples. Other images are skipped, and the panel warns about them.

## Road types
A lane's road types are checked against the category of the road being built:
* **Requires all of** (`RequireAll`): every selected type must be part of the road.
* **Requires one of** (`RequireAny`): at least one selected type must be part of the road. Empty means no requirement.
* **Not allowed in** (`RequireNone`): roads with any selected type can't use the lane.

Plain asphalt roads have **no** category flag. A lane that requires any type is never allowed on plain roads, so to limit a lane to plain roads (or plain roads and a few others), exclude the types you don't want instead.
The panel's simple **Allowed in** picker handles this for you; switch to **Advanced category rules** to edit the three lists directly.

## Component reference
### RoadBuilderLaneInfo
Required for a section to appear in the lane list on its own. Optional on group variants and lane groups.

| Field | What it does |
|---|---|
| `RequireAll`, `RequireAny`, `RequireNone` | Road types, see above. On a group variant they decide which variant is used for which road type. |
| `FrontThumbnail`, `BackThumbnail` | Lane bar icon. The back icon is shown for lanes going forward and the front icon for lanes going backward; each falls back to the other. Without either, the section's own icon is used. |
| `LaneColor` | Tints the lane in the lane bar. Leave fully transparent for the road's surface color. |
| `GroundTexture` | Lane bar surface: asphalt, painted asphalt, gravel, tiled or rail. *Unset* picks one from the road and the lane. |
| `NoDirection` | Hides the direction option and arrow, for lanes like medians, sidewalks or parking. |
| `Median` | Marks the section as a median in generated roads. |
| `LaneThumbnails` | Road thumbnail images, from left to right. Without any, a generic car lane is drawn. |
| `PieceRequireAll`, `PieceRequireAny`, `PieceRequireNone` | Limits the section to some road states (elevated, tunnel, with an upgrade...). Most lanes don't need this. |

On a lane group, the group's lane info is used for its entry in the lane list, and for any variant that doesn't have its own.

### RoadBuilderEdgeLaneInfo
Makes the lane an edge lane. Can be added to a section or to a lane group.

| Field | What it does |
|---|---|
| `LeftSidePrefab`, `RightSidePrefab` | The section added beyond the lane when it's the outermost lane, like a curb. Empty uses the road type's default side. |
| `AddSidewalkStateOnNode` | Gives intersections the sidewalk corners and crosswalks of a road with sidewalks. |
| `DoNotRequireBeingOnEdge` | Lets the lane be placed between other lanes too, like a sound barrier. |

### RoadBuilderLaneGroup
Makes the section a variant of a lane group. The section is then hidden from the lane list and reached through the group.

| Field | What it does |
|---|---|
| `GroupPrefab` | The lane group. If it isn't loaded, the section is listed as a normal lane instead (if it has lane info). |
| `Combination` | The option values that lead to this variant, as pairs of option name and value. See [Matching variants](#matching-variants). |

### LaneGroupPrefab
A lane group, listed as one lane with options. Its `Options` are shown when the lane is selected in Road Builder, in order. Each option has:

| Field | What it does |
|---|---|
| `Name` | Required and unique within the group, variants refer to options by name. Also the key saved with players' roads, so avoid renaming it once published. |
| `Type` | How the option is shown, see the table below. |
| `DefaultValue` | Used when a lane is first added. |
| `IgnoreForSimilarDuplicate` | Normally a lane added next to the same kind of lane copies its options; this option starts at its default instead. |
| `Options` | The option's values, each with a `Value` and a button icon (`ThumbnailUrl`). Values without an icon show their name instead. |

| Option type | Shown as | Variant value |
|---|---|---|
| Buttons, Multi-select buttons, Toggle | One button per value. Multi-select currently behaves like single select. | One of the option's values. |
| Up/down value, Lane width | Arrows stepping through the values, in order. Lane width values like `3.5m` are shown in feet for imperial users. | One of the option's values. |
| Checkbox | A single checkbox. | Any value for the checked variant, nothing for the unchecked one. |
| Grass & trees (Decoration) | Grass and tree toggles. Doesn't pick a variant, every variant can support it. | Nothing for no decorations, `G` grass only, `T` trees only, anything else (like `GT`) both. |
| Two-way | Adds a **Two-way** button to the lane's direction option. | Any value except `0` (the panel uses `1`) for the two-way variant, nothing for the one-way one. |

Each option's buttons are filtered by the options before it, so put the options that matter most first.

### RoadBuilderLaneDecorationInfo
Icons for groups with a **Grass & trees** option: a lane icon and road thumbnail images for grass, trees, and both. On a variant or on the group; the variant's are used first. Ignored on standalone lanes.

### RoadBuilderLaneAggregate
Extra sections placed beside the lane in generated roads, like the shoulder next to a parking lane. Each entry picks a section, whether it's flipped relative to the lane, and optional road-state requirements. Usually paired with `RoadBuilderIgnoreSection` on the extra section.

### RoadBuilderIgnoreSection
Leaves the section out when a road that contains it is opened in Road Builder (imported). It doesn't hide the section from the lane list.

## Matching variants
* Option names and values are matched **without case**.
* A variant that leaves an option out counts as having an **empty** value for it.
* When no variant matches a player's saved options exactly (for example after you renamed or removed a value in an update), Road Builder uses the closest variant so the lane stays in the road.
* If two variants have the same values and road types, only the first one can be used. The panel warns about this.

## Good to know
* **Updating your lanes**: lanes are saved by prefab name, group name, option name and value. Renaming any of them in an update makes existing roads fall back to defaults or the closest variant.
* **Removing a lane mod**: roads that use its lanes are rebuilt without them. The lanes come back if the mod is installed again.
* **Variants in Road Builder's own groups** can only use the values those groups already have. To offer new values, make your own lane group.
* **Lane list icon**: the lane list always shows the prefab's own icon; the lane info icons are for the lane bar.
* **Names**: prefab and group names are global. Avoid names starting with `RB ` or matching Road Builder's own lanes.
* Some behavior is built in and can't be changed by custom lanes: the empty median added to roads with an even number of lanes, the default road sides, and the lane bar's surface fallback.
* Problems with installed custom lanes are written to Road Builder's log as `[Custom lane]` warnings.


# FAQ
### Can I combine Subway tracks with other kinds of transportation?
The game does not like to mix Subway networks with other kinds of networks. For this reason, you can’t use Subway lanes in other types of networks by default in Road Builder. However, you can enable “Advanced User Mode” to unlock this restriction.

You will still however need to fiddle around to use those networks in the game.


### Can I make custom bridges with Road Builder?
Bridges that have a unique style are not supported. In the future, there might be minimal support for these kinds of bridges.


### What happens if I share my save-game with someone else?

As long as someone has Road Builder, they will be able to load save-games that contain custom roads in them with no extra steps. It will just work.


### What happens if I use cloud save and load the save-game from another computer?

Just like sharing save-games, you can load your save-game on another computer with no extra steps. However, you might run into an issue where road configurations are duplicated because roads are still saved locally. A good idea would be to turn on “Only save used roads” in the Road Builder options.


### How can I share roads with someone else?

Sharing roads is possible, but it is a manual process for now. You can find your road configurations inside:
C:\Users\%username%\AppData\LocalLow\Colossal Order\Cities Skylines II\ModsData\RoadBuilder

Copy the selected road files from your computer to the same location on the other user’s computer. Then they will find the roads in their game automatically.


### Sharing saves on Paradox Mods

When uploading a save to Paradox Mods that contains custom roads from Road Builder, remember to set Road Builder as a required item. You can do this by navigating to Paradox Mods on your web browser;
- finding the save under “My Uploaded Mods”
- edit the current version
- navigate to the “version” tab
- find Road Builder under the “Required Mods” drop down
- save changes


### What happens if I load a save using Road Builder without Road Builder installed?

The save should load, though all custom roads will load in as default grey networks which you can replace with base game roads.


# Help and Support
If you're facing any issues with the mod, please use the discord linked on the right.
This helps us better assist you and avoids your message from getting lost in the comments.
