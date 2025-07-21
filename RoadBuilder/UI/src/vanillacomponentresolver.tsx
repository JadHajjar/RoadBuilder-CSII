import { Bounds1, Color, Theme, UniqueFocusKey } from "cs2/bindings";
import { InputAction } from "cs2/input";
import { ModuleRegistry } from "cs2/modding";
import { BalloonDirection, FocusKey, PanelTheme, ScrollController } from "cs2/ui";
import { CSSProperties, EventHandler, HTMLAttributes, KeyboardEventHandler, MouseEventHandler, MutableRefObject, ReactNode } from "react";

// These are specific to the types of components that this mod uses.
// In the UI developer tools at http://localhost:9444/ go to Sources -> Index.js. Pretty print if it is formatted in a single line.
// Search for the tsx or scss files. Look at the function referenced and then find the properies for the component you're interested in.
// As far as I know the types of properties are just guessed.
type PropsToolButton = {
  focusKey?: UniqueFocusKey | null;
  src: string;
  selected?: boolean;
  multiSelect?: boolean;
  disabled?: boolean;
  tooltip?: string | JSX.Element | null;
  selectSound?: any;
  uiTag?: string;
  className?: string;
  children?: string | JSX.Element | JSX.Element[];
  onSelect?: (x: any) => any;
} & HTMLAttributes<any>;

type PropsSection = {
  title?: string | null;
  uiTag?: string;
  children: string | JSX.Element | JSX.Element[];
};

type ToggleProps = {
  focusKey?: FocusKey;
  checked?: boolean;
  disabled?: boolean;
  style?: CSSProperties;
  className?: string;
  children?: ReactNode;
  onChange?: () => void;
  onMouseOver?: () => void;
  onMouseLeave?: () => void;
};

type Checkbox = {
  checked?: boolean;
  disabled?: boolean;
  className?: string;
  theme?: any;
} & HTMLAttributes<any>;

// var C4 = function() {

// Use of it
/** From the transport-line-item.tsx
 *  z.jsx)("div", {
        className: Zve.cellSingle,
        children: (0,
        z.jsx)(Tp, {
            tooltip: (0,
            z.jsx)(Zc.Transport.TOOLTIP_COLOR, {
                hash: x
            }),
            children: (0,
            z.jsx)(C4, {
                value: n.lineData.color,
                className: nbe.colorField,
                onChange: f,
                onClick: lv
            })
        })
    })
 */
type ColorField = {
  focusKey?: FocusKey;
  disabled?: boolean;
  value?: Color;
  className?: string;
  selectAction?: InputAction;
  alpha?: any;
  popupDirection?: BalloonDirection;
  onChange?: (e: Color) => void;
  onClick?: (e: any) => void;
  onMouseEnter?: (e: any) => void;
  onMouseLeave?: (e: any) => void;
};

type Bounds1InputField = {
  label: any;
  value: Bounds1;
  allowMinGreaterMax: boolean;
  disabled?: boolean;
  onChange: (bounds: Bounds1) => void;
  onChangeStart?: (e: any) => void;
  onChangeEnd?: (e: any) => void;
};

// jG({value: e, valueFormatter: t, inputValidator: n, inputTransformer: s=CG, inputParser: a, onChange: i, onFocus: o, onBlur: r, ...l})
type HexColorInput = {
  value: any;
  onFocus?: any;
  onBlur?: any;
  onChange?: (text: string) => void;
};

// function P4(e) {
// type BoundColorField = {
//     value?: any;
//     disabled?: boolean;
// }

//({h: e, s: t=1, v: n=1, decimalPrecision: s=3, outerRadius: a, innerRadius: i, className: o, onChange: r, onDragStart: l, onDragEnd: c})=>{
type RadialHuePicker = {
  h: number; // the hue
  s?: number;
  v?: number;
  decimalPrecision?: number;
  outerRadius: number;
  innerRadius: number;
  className: string;
  onChange: (e: number) => void;
  onDragStart: (e: any) => void;
  onDragEnd: (e: any) => void;
};

export type ColorPickerPreview = "None" | "Current" | "CurrentAndLast";
export type ColorPickerSliderMode = "Hsv" | "RgbFloat" | "RgbByte";
export interface ColorHSV {
  h: number; // between 0 and 1
  s: number; // between 0 and 1
  v: number; // between 0 and 1
  a?: number; // between 0 and 1
}
//qB = ({focusKey: e, color: t, alpha: n, colorWheel: s=!0, sliderTextInput: a=!0, preview: i=$B.None, mode: o, hexInput: r=!0, onChange: l})=>{
type ColorPicker = {
  focusKey?: any;
  color: ColorHSV; // between 0 and 1
  alpha?: number; // between 0 and 1
  colorWheel?: boolean; // defaults to false
  sliderTextInput?: boolean; // defaults to false
  preview?: ColorPickerPreview;
  mode?: ColorPickerSliderMode; // defaults to Hsv
  hexInput?: boolean; // defaults to false
  onChange?: (color: ColorHSV) => void;
};

type DataInput = {
  //idk what this should be named
  value: any;
  valueFormatter: () => string;
  inputValidator: (text: string) => boolean;
  inputTransformer?: (text: string) => string;
  inputParser: (text: string, t: number, n: number) => any;
  onChange?: (text: string) => void;
  onFocus?: (e: any) => void;
  onBlur?: (e: any) => void;
};

type IntInput = {
  min?: number;
  max?: number;
  className: string;
} & Partial<DataInput>;

type BoundIntInputField = IntInput;

type TextInputTheme = {
  input: string;
  label: string;
  container: string;
};

type EllipsisTextInput = {
  value: string | undefined;
  maxLength?: number; // default is 64
  theme?: any;
  className?: string;
  vkTitle?: string;
  placeholder?: string;
  ref?: MutableRefObject<HTMLInputElement>;
  focusKey?: FocusKey;
  onClick?: MouseEventHandler;
  onKeyDown?: KeyboardEventHandler<HTMLInputElement>;
  onChange?: (event: React.ChangeEvent<HTMLInputElement>) => void;
  onFocus?: () => void;
  onBlur?: () => void;
};

type EllipsesTextInputTheme = {
  "ellipses-text-input": string;
  ellipsesTextInput: string;
  input: string;
  label: string;
};

// This is an array of the different components and sass themes that are appropriate for your UI. You need to figure out which ones you need from the registry.
const registryIndex = {
  Section: ["game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", "Section"],
  ToolButton: ["game-ui/game/components/tool-options/tool-button/tool-button.tsx", "ToolButton"],
  toolButtonTheme: ["game-ui/game/components/tool-options/tool-button/tool-button.module.scss", "classes"],
  Toggle: ["game-ui/common/input/toggle/toggle.tsx", "Toggle"],
  toggleTheme: ["game-ui/menu/widgets/toggle-field/toggle-field.module.scss", "classes"],
  Checkbox: ["game-ui/common/input/toggle/checkbox/checkbox.tsx", "Checkbox"],
  checkboxTheme: ["game-ui/common/input/toggle/checkbox/checkbox.module.scss", "classes"],
  ColorField: ["game-ui/common/input/color-picker/color-field/color-field.tsx", "ColorField"],
  BoundColorField: ["game-ui/common/input/color-picker/color-field/color-field.tsx", "BoundColorField"],
  Bounds1InputField: ["game-ui/editor/widgets/fields/bounds-field.tsx", "Bounds1InputField"], // rB
  HexColorInput: ["game-ui/common/input/text/hex-color-input.tsx", "HexColorInput"], //extends jG
  RadialHuePicker: ["game-ui/common/input/color-picker/radial-hue-picker/radial-hue-picker.tsx", "RadialHuePicker"],
  ColorPickerPreview: ["game-ui/common/input/color-picker/color-picker/color-picker.tsx", "ColorPickerPreview"],
  ColorPickerSliderMode: ["game-ui/common/input/color-picker/color-picker/color-picker.tsx", "ColorPickerSliderMode"],
  ColorPicker: ["game-ui/common/input/color-picker/color-picker/color-picker.tsx", "ColorPicker"],
  IntInput: ["game-ui/common/input/text/int-input.tsx", "IntInput"],
  BoundIntInputField: ["game-ui/game/widgets/field/int-input-field.tsx", "BoundIntInputField"],
  textInputTheme: ["game-ui/game/components/selected-info-panel/shared-components/text-input/text-input.module.scss", "classes"],
  mouseToolOptionsTheme: ["game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.module.scss", "classes"],
  ellipsesTextInputTheme: ["game-ui/common/input/text/ellipsis-text-input/ellipsis-text-input.module.scss", "classes"],
  EllipsisTextInput: ["game-ui/common/input/text/ellipsis-text-input/ellipsis-text-input.tsx", "EllipsisTextInput"],
  assetGridTheme: ["game-ui/game/components/item-grid/item-grid.module.scss", "classes"],

  FOCUS_DISABLED: ["game-ui/common/focus/focus-key.ts", "FOCUS_DISABLED"],
  FOCUS_AUTO: ["game-ui/common/focus/focus-key.ts", "FOCUS_AUTO"],
  useUniqueFocusKey: ["game-ui/common/focus/focus-key.ts", "useUniqueFocusKey"],
  useScrollController: ["game-ui/common/hooks/use-scroll-controller.tsx", "useScrollController"],
};

export class VanillaComponentResolver {
  // As far as I know you should not need to edit this portion here.
  // This was written by Klyte for his mod's UI but I didn't have to make any edits to it at all.
  public static get instance(): VanillaComponentResolver {
    return this._instance!!;
  }
  private static _instance?: VanillaComponentResolver;

  public static setRegistry(in_registry: ModuleRegistry) {
    this._instance = new VanillaComponentResolver(in_registry);
  }
  private registryData: ModuleRegistry;

  constructor(in_registry: ModuleRegistry) {
    this.registryData = in_registry;
  }

  private cachedData: Partial<Record<keyof typeof registryIndex, any>> = {};
  private updateCache(entry: keyof typeof registryIndex) {
    const entryData = registryIndex[entry];
    return (this.cachedData[entry] = this.registryData.registry.get(entryData[0])!![entryData[1]]);
  }

  // This section defines your components and themes in a way that you can access via the singleton in your components.
  // Replace the names, props, and strings as needed for your mod.
  public get Section(): (props: PropsSection) => JSX.Element {
    return this.cachedData["Section"] ?? this.updateCache("Section");
  }
  public get ToolButton(): (props: PropsToolButton) => JSX.Element {
    return this.cachedData["ToolButton"] ?? this.updateCache("ToolButton");
  }
  public get Toggle(): (props: ToggleProps) => JSX.Element {
    return this.cachedData["Toggle"] ?? this.updateCache("Toggle");
  }
  public get Checkbox(): (props: Checkbox) => JSX.Element {
    return this.cachedData["Checkbox"] ?? this.updateCache("Checkbox");
  }
  public get ColorField(): (props: ColorField) => JSX.Element {
    return this.cachedData["ColorField"] ?? this.updateCache("ColorField");
  }
  public get HexColorInput(): (props: HexColorInput) => JSX.Element {
    return this.cachedData["HexColorInput"] ?? this.updateCache("HexColorInput");
  }
  public get Bounds1InputField(): (props: Bounds1InputField) => JSX.Element {
    return this.cachedData["Bounds1InputField"] ?? this.updateCache("Bounds1InputField");
  }
  public get RadialHuePicker(): (props: RadialHuePicker) => JSX.Element {
    return this.cachedData["RadialHuePicker"] ?? this.updateCache("RadialHuePicker");
  }
  public get IntInput(): (props: IntInput) => JSX.Element {
    return this.cachedData["IntInput"] ?? this.updateCache("IntInput");
  }
  public get BoundIntInputField(): (props: BoundIntInputField) => JSX.Element {
    return this.cachedData["BoundIntInputField"] ?? this.updateCache("BoundIntInputField");
  }
  public get EllipsisTextInput(): (props: EllipsisTextInput) => JSX.Element {
    return this.cachedData["EllipsisTextInput"] ?? this.updateCache("EllipsisTextInput");
  }

  // Broken
  // public get ColorPicker() : (props: ColorPicker) => JSX.Element { return this.cachedData['ColorPicker'] ?? this.updateCache("ColorPicker")};

  public get toggleTheme(): Theme | any {
    return this.cachedData["toggleTheme"] ?? this.updateCache("toggleTheme");
  }
  public get checkboxTheme(): Theme | any {
    return this.cachedData["checkboxTheme"] ?? this.updateCache("checkboxTheme");
  }
  public get mouseToolOptionsTheme(): Theme | any {
    return this.cachedData["mouseToolOptionsTheme"] ?? this.updateCache("mouseToolOptionsTheme");
  }
  public get toolButtonTheme(): Theme | any {
    return this.cachedData["toolButtonTheme"] ?? this.updateCache("toolButtonTheme");
  }
  public get textInputTheme(): TextInputTheme | Theme | any {
    return this.cachedData["textInputTheme"] ?? this.updateCache("textInputTheme");
  }
  public get ellipsesTextInputTheme(): EllipsesTextInputTheme | Theme | any {
    return this.cachedData["ellipsesTextInputTheme"] ?? this.updateCache("ellipsesTextInputTheme");
  }
  public get assetGridTheme(): Theme | any {
    return this.cachedData["assetGridTheme"] ?? this.updateCache("assetGridTheme");
  }

  public get FOCUS_DISABLED(): UniqueFocusKey {
    return this.cachedData["FOCUS_DISABLED"] ?? this.updateCache("FOCUS_DISABLED");
  }
  public get FOCUS_AUTO(): UniqueFocusKey {
    return this.cachedData["FOCUS_AUTO"] ?? this.updateCache("FOCUS_AUTO");
  }
  public get useUniqueFocusKey(): (focusKey: FocusKey, debugName: string) => UniqueFocusKey | null {
    return this.cachedData["useUniqueFocusKey"] ?? this.updateCache("useUniqueFocusKey");
  }
  public get useScrollController(): () => ScrollController {
    return this.cachedData["useScrollController"] ?? this.updateCache("useScrollController");
  }
}
