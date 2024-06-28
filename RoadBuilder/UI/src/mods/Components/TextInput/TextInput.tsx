import { Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { FocusKey } from "cs2/ui";

export interface TextInputProps {
    focusKey?: FocusKey;
    debugName?: string;
    type?: "text" | "password";
    value?: string;
    selectAllOnFocus?: boolean;
    placeholder?: string;
    vkTitle?: string;
    vkDescription?: string;
    disabled?: boolean;
    className?: string;
    multiline: number;
    ref?: any;
    onFocus?: (value: Event) => void;
    onBlur?: (value: Event) => void;
    onKeyDown?: (value: Event) => void;
    onChange?: (value: React.ChangeEvent<HTMLInputElement>) => void;
    onMouseUp?: (value: Event) => void;
} 

export const TextInput : (props: TextInputProps) => JSX.Element = getModule(
    "game-ui/common/input/text/text-input.tsx",
    "TextInput"
  );

export const TextInputTheme: Theme | any = getModule(
    "game-ui/editor/widgets/item/editor-item.module.scss",
    "classes"
  );

export const AssetGridTheme: Theme | any = getModule(
    "game-ui/game/components/asset-menu/asset-grid/asset-grid.module.scss",
    "classes"
  );