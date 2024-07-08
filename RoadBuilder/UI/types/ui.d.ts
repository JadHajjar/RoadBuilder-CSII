declare module "cs2/ui" {
  import { CSSProperties, HTMLAttributes, PropsWithChildren, ReactElement, ReactNode, Ref } from 'react';
  
  export interface RefReactElement<T = any, P = any> extends ReactElement<P> {
  	ref?: Ref<T>;
  }
  export interface TransitionStyles {
  	base?: string;
  	enter?: string;
  	enterActive?: string;
  	exit?: string;
  	exitActive?: string;
  }
  export interface ClassProps {
  	className?: string;
  }
  export type BalloonDirection = "up" | "down" | "left" | "right";
  export type BalloonAlignment = "start" | "center" | "end";
  export interface BalloonTheme extends TransitionStyles {
  	balloon: string;
  	bounds: string;
  	container: string;
  	arrow: string;
  	content: string;
  	hidden: string;
  	up: string;
  	down: string;
  	left: string;
  	right: string;
  	start: string;
  	center: string;
  	end: string;
  }
  export interface TooltipProps extends ClassProps {
  	tooltip: ReactNode;
  	disabled?: boolean;
  	forceVisible?: boolean;
  	theme?: Partial<BalloonTheme>;
  	direction?: BalloonDirection;
  	alignment?: BalloonAlignment;
  	children: RefReactElement;
  }
  export export const Tooltip: ({ tooltip, forceVisible, disabled, theme, direction, alignment, className, children }: PropsWithChildren<TooltipProps>) => JSX.Element;
  export const FOCUS_DISABLED: unique symbol;
  export const FOCUS_AUTO: unique symbol;
  export type FocusKey = typeof FOCUS_DISABLED | typeof FOCUS_AUTO | UniqueFocusKey;
  export type UniqueFocusKey = FocusSymbol | string | number;
  export class FocusSymbol {
  	readonly debugName: string;
  	readonly r: number;
  	constructor(debugName: string);
  	toString(): string;
  }
  export interface PanelTheme extends PanelTitleBarTheme {
  	panel: string;
  	header: string;
  	content: string;
  	footer: string;
  }
  export interface PanelTitleBarTheme {
  	titleBar: string;
  	title: string;
  	icon: string;
  	iconSpace: string;
  	closeButton: string;
  	closeIcon: string;
  	toggle: string;
  	toggleIcon: string;
  	toggleIconExpanded: string;
  }
  export interface DialogStackProps {
  	showDialog: (dialog: ReactNode) => void;
  	closeAll: () => void;
  }
  export export const DialogStack: import("react").Context<DialogStackProps>;
  export interface DialogContextProps {
  	onClose: () => void;
  }
  export export const DialogContext: import("react").Context<DialogContextProps>;
  export export const DialogRenderer: ({ children }: PropsWithChildren) => JSX.Element;
  export interface ConfirmationDialogProps {
  	title?: ReactNode;
  	message: ReactNode;
  	details?: string;
  	confirm?: ReactNode;
  	cancel?: ReactNode;
  	onConfirm: (dismiss: boolean) => void;
  	onCancel?: () => void;
  	dismissable?: boolean;
  	cancellable?: boolean;
  	zIndex?: number;
  }
  export const UITriggeredConfirmationDialog: React.FC<ConfirmationDialogProps>;
  export enum UISound {
  	selectItem = "select-item",
  	dragSlider = "drag-slider",
  	hoverItem = "hover-item",
  	expandPanel = "expand-panel",
  	grabSlider = "grabSlider",
  	selectDropdown = "select-dropdown",
  	selectToggle = "select-toggle",
  	focusInputField = "focus-input-field",
  	signatureBuildingEvent = "signature-building-event",
  	bulldoze = "bulldoze",
  	bulldozeEnd = "bulldoze-end",
  	relocateBuilding = "relocate-building",
  	mapTilePurchaseMode = "map-tile-purchase-mode",
  	mapTilePurchaseModeEnd = "map-tile-purchase-mode-end",
  	xpEvent = "xp-event",
  	milestoneEvent = "milestone-event",
  	economy = "economy",
  	chirpEvent = "chirp-event",
  	likeChirp = "like-chirp",
  	chirper = "chirper",
  	purchase = "purchase",
  	enableBuilding = "enable-building",
  	disableBuilding = "disable-building",
  	pauseSimulation = "pause-simulation",
  	resumeSimulation = "resume-simulation",
  	simulationSpeed1 = "simulation-speed-1",
  	simulationSpeed2 = "simulation-speed-2",
  	simulationSpeed3 = "simulation-speed-3",
  	togglePolicy = "toggle-policy",
  	takeLoan = "take-loan",
  	removeItem = "remove-item",
  	toggleInfoMode = "toggle-info-mode",
  	takePhoto = "take-photo",
  	tutorialTriggerCompleteEvent = "tutorial-trigger-complete-event",
  	selectRadioNetwork = "select-radio-network",
  	selectRadioStation = "select-radio-station",
  	generateRandomName = "generate-random-name",
  	decreaseElevation = "decrease-elevation",
  	increaseElevation = "increase-elevation",
  	selectPreviousItem = "select-previous-item",
  	selectNextItem = "select-next-item",
  	openPanel = "open-panel",
  	closePanel = "close-panel",
  	openMenu = "open-menu",
  	closeMenu = "close-menu"
  }
  export interface Number2 {
  	readonly x: number;
  	readonly y: number;
  }
  export type Action = () => void | boolean;
  export type Action1D = (value: number) => void | boolean;
  export interface InputActionsDefinition {
  	"Move Horizontal": Action1D;
  	"Change Slider Value": Action1D;
  	"Change Tool Option": Action1D;
  	"Change Value": Action1D;
  	"Move Vertical": Action1D;
  	"Switch Radio Station": Action1D;
  	"Scroll Vertical": Action1D;
  	"Select": Action;
  	"Purchase Dev Tree Node": Action;
  	"Select Chirp Sender": Action;
  	"Save Game": Action;
  	"Expand Group": Action;
  	"Collapse Group": Action;
  	"Select Route": Action;
  	"Remove Operating District": Action;
  	"Upgrades Menu": Action;
  	"Purchase Map Tile": Action;
  	"Unfollow Citizen": Action;
  	"Like Chirp": Action;
  	"Unlike Chirp": Action;
  	"Enable Info Mode": Action;
  	"Disable Info Mode": Action;
  	"Toggle Tool Color Picker": Action;
  	"Cinematic Mode": Action;
  	"Photo Mode": Action;
  	"Back": Action;
  	"Leave Underground Mode": Action;
  	"Leave Info View": Action;
  	"Switch Tab": Action1D;
  	"Switch Option Section": Action1D;
  	"Switch DLC": Action1D;
  	"Switch Ordering": Action1D;
  	"Switch Radio Network": Action1D;
  	"Change Time Scale": Action1D;
  	"Switch Page": Action1D;
  	"Tool Options": Action;
  	"Switch Toolmode": Action;
  	"Toggle Snapping": Action;
  	"Capture Keyframe": Action;
  	"Reset Property": Action;
  	"Toggle Property": Action;
  	"Previous Tutorial Phase": Action;
  	"Continue Tutorial": Action;
  	"Focus Tutorial List": Action;
  	"Pause Simulation": Action;
  	"Resume Simulation": Action;
  	"Switch Speed": Action;
  	"Speed 1": Action;
  	"Speed 2": Action;
  	"Speed 3": Action;
  	"Bulldozer": Action;
  	"Change Elevation": Action1D;
  	"Advisor": Action;
  	"Quicksave": Action;
  	"Quickload": Action;
  	"Focus Selected Object": Action;
  	"Hide UI": Action;
  	"Map tile Purchase Panel": Action;
  	"Info View": Action;
  	"Progression Panel": Action;
  	"Economy Panel": Action;
  	"City Information Panel": Action;
  	"Statistic Panel": Action;
  	"Transportation Overview Panel": Action;
  	"Chirper Panel": Action;
  	"Lifepath Panel": Action;
  	"Event Journal Panel": Action;
  	"Radio Panel": Action;
  	"Photo Mode Panel": Action;
  	"Take Photo": Action;
  	"Pause Menu": Action;
  	"Load Game": Action;
  	"Start Game": Action;
  	"Save Options": Action;
  	"Switch User": Action;
  	"Unset Binding": Action;
  	"Reset Binding": Action;
  	"Switch Savegame Location": Action1D;
  	"Show Advanced": Action;
  	"Hide Advanced": Action;
  	"Select Directory": Action;
  	"Search Options": Action;
  	"Clear Search": Action;
  	"Debug UI": Action;
  	"Debug Prefab Tool": Action;
  	"Debug Change Field": Action1D;
  	"Debug Multiplier": Action1D;
  }
  export type InputAction = keyof InputActionsDefinition;
  export interface ButtonTheme {
  	button: string;
  }
  export interface ButtonSounds {
  	select?: UISound | string | null;
  	hover?: UISound | string | null;
  	focus?: UISound | string | null;
  }
  export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement | HTMLDivElement> {
  	focusKey?: FocusKey;
  	debugName?: string;
  	selected?: boolean;
  	theme?: Partial<ButtonTheme>;
  	sounds?: ButtonSounds | null;
  	selectAction?: InputAction;
  	selectSound?: UISound | string | null;
  	tooltipLabel?: ReactNode;
  	/** When the button is clicked or the SELECT button on a gamepad is pressed */
  	onSelect?: () => void;
  	as?: "button" | "div";
  }
  export interface IconButtonTheme extends ButtonTheme {
  	icon: string;
  }
  export interface IconButtonProps extends ButtonProps {
  	src: string;
  	tinted?: boolean;
  	theme?: Partial<IconButtonTheme>;
  }
  export interface LabeledIconButtonTheme extends IconButtonTheme {
  	label: string;
  }
  export interface LabeledIconButtonProps extends IconButtonProps {
  	theme: Partial<LabeledIconButtonTheme>;
  }
  type ButtonTheme$1 = Partial<LabeledIconButtonTheme>;
  type ButtonProps$1 = ButtonProps & Partial<LabeledIconButtonProps> & {
  	variant?: "flat" | "primary" | "round" | "menu" | "icon" | "floating" | "default";
  	theme?: ButtonTheme$1;
  };
  export export const Button: (props: ButtonProps$1) => JSX.Element;
  export export const MenuButton: (props: Partial<LabeledIconButtonProps>) => JSX.Element;
  export export const FloatingButton: (props: Partial<IconButtonProps>) => JSX.Element;
  export type AnchoredPopupAlignment = "left" | "right";
  export interface DropdownTheme extends DropdownToggleTheme, DropdownMenuTheme, DropdownItemTheme {
  }
  export interface DropdownToggleTheme {
  	dropdownToggle: string;
  	label: string;
  	indicator?: string;
  	hiddenIcon?: string;
  	visibleIcon?: string;
  }
  export interface DropdownMenuTheme {
  	dropdownPopup: string;
  	dropdownMenu: string;
  	scrollable: string;
  }
  export interface DropdownItemTheme {
  	dropdownItem: string;
  }
  export interface DropdownProps {
  	focusKey?: FocusKey;
  	initialFocused?: UniqueFocusKey | null;
  	theme?: Partial<DropdownTheme>;
  	content: ReactNode;
  	alignment?: AnchoredPopupAlignment;
  }
  export export const Dropdown: ({ focusKey, initialFocused, theme: partialTheme, content, alignment, children }: PropsWithChildren<DropdownProps>) => JSX.Element;
  export interface DropdownToggleProps extends DropdownToggleBaseProps {
  	theme?: DropdownToggleTheme;
  }
  export export const DropdownToggle: ({ theme, children, ...props }: PropsWithChildren<DropdownToggleProps>) => JSX.Element;
  export interface DropdownToggleBaseProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  	tooltip?: ReactNode;
  	theme?: Partial<DropdownToggleTheme>;
  	sounds?: ButtonSounds | null;
  	selectSound?: UISound | string | null;
  	tooltipLabel?: ReactNode;
  }
  export interface DropdownItemProps<T> extends ClassProps {
  	focusKey?: FocusKey;
  	value: T;
  	selected?: boolean;
  	theme?: DropdownItemTheme;
  	sounds?: ButtonSounds | null;
  	closeOnSelect?: boolean;
  	/**
  	 * Called when the user selects this item.
  	 * The provided callback will not be invoked if this item is already selected.
  	 */
  	onChange?: (value: T) => void;
  	/** Called when the user clicks this item while it was already selected. */
  	onToggleSelected?: (value: T) => void;
  }
  /**
   * Used to select between a number of mutually exclusive values.
   * When one item is selected, the other items in the menu cease to be selected.
   *
   * Values can be integers, enums, strings or any other object that can be compared with `===`.
   *
   * When the item is clicked, the dropdown menu is automatically hidden.
   */
  export export function DropdownItem<T>({ focusKey, value, selected, theme, sounds, className, onChange, onToggleSelected, closeOnSelect, children }: PropsWithChildren<DropdownItemProps<T>>): JSX.Element;
  export interface TransitionSounds {
  	enter?: UISound | string | null;
  	exit?: UISound | string | null;
  }
  export interface PanelProps extends HTMLAttributes<HTMLDivElement> {
  	focusKey?: FocusKey;
  	header?: ReactNode;
  	footer?: ReactNode;
  	theme?: Partial<PanelTheme>;
  	transition?: TransitionStyles | null;
  	transitionSounds?: TransitionSounds | null;
  	contentClassName?: string;
  	onClose?: () => void;
  	allowFocusExit?: boolean;
  }
  export interface DraggablePanelProps extends PanelProps {
  	initialPosition?: Number2;
  }
  export interface InfoSectionProps extends ClassProps {
  	focusKey?: FocusKey;
  	tooltip?: ReactNode;
  	disableFocus?: boolean;
  }
  export const InfoSection: ({ focusKey, tooltip, disableFocus, className, children }: PropsWithChildren<InfoSectionProps>) => JSX.Element;
  export interface InfoSectionFoldoutProps extends InfoSectionProps {
  	header?: ReactNode;
  	initialExpanded?: boolean;
  	expandFromContent?: boolean;
  	onToggleExpanded?: (expanded: boolean) => void;
  }
  export const InfoSectionFoldout: ({ header, initialExpanded, expandFromContent, focusKey, tooltip, disableFocus, className, onToggleExpanded, children }: PropsWithChildren<InfoSectionFoldoutProps>) => JSX.Element;
  export interface InfoRowProps extends ClassProps {
  	icon?: string;
  	left?: ReactNode;
  	right?: ReactNode;
  	tooltip?: ReactNode;
  	link?: ReactNode;
  	uppercase?: boolean;
  	subRow?: boolean;
  	disableFocus?: boolean;
  }
  export const InfoRow: ({ icon, left, right, tooltip, link, uppercase, subRow, disableFocus, className }: InfoRowProps) => JSX.Element;
  export interface SimplePanelProps extends PanelProps {
  	draggable?: false | undefined;
  }
  interface DraggablePanelProps$1 extends DraggablePanelProps {
  	draggable: true;
  }
  type PanelProps$1 = SimplePanelProps | DraggablePanelProps$1;
  export export const Panel: (props: PropsWithChildren<PanelProps$1>) => JSX.Element;
  export interface IconProps {
  	src: string;
  	tinted?: boolean;
  	className?: string;
  }
  export export const Icon: ({ tinted, className, src }: IconProps) => JSX.Element;
  export const PortalContainerProvider: ({ children }: {
  	children: RefReactElement<HTMLElement>;
  }) => JSX.Element;
  export function usePortalContainer(): HTMLElement;
  export interface IPortal {
  	({ children }: PropsWithChildren): React.ReactPortal;
  	usePortalContainer: typeof usePortalContainer;
  	ContainerProvider: typeof PortalContainerProvider;
  }
  export export const Portal: IPortal;
  export interface AutoScrollSettings {
  	speed: number;
  	delay: number;
  	repeat: boolean;
  	minOverflow: number;
  }
  export interface ScrollControllerCallback {
  	scrollTo(x: number, y: number): void;
  	scrollBy(x: number, y: number): void;
  	smoothScrollTo(x: number, y: number): void;
  	scrollIntoView(element: Element): void;
  }
  export class ScrollController {
  	private _callback;
  	scrollTo(x: number, y: number): void;
  	scrollBy(x: number, y: number): void;
  	smoothScrollTo(x: number, y: number): void;
  	scrollIntoView(element: Element): void;
  	_attachCallback(callback: ScrollControllerCallback): void;
  	_detachCallback(callback: ScrollControllerCallback): void;
  }
  export interface ScrollableProps {
  	horizontal?: boolean;
  	vertical?: boolean;
  	trackVisibility?: "always" | "scrollable";
  	overshootX?: number;
  	overshootY?: number;
  	smooth?: boolean;
  	controller?: ScrollController;
  	className?: string;
  	style?: CSSProperties;
  	onScroll?: () => void;
  	onOverflowX?: (overflow: boolean) => void;
  	onOverflowY?: (overflow: boolean) => void;
  	autoScroll?: boolean;
  	autoScrollSettings?: AutoScrollSettings;
  }
  export export const Scrollable: (props: ScrollableProps & {
  	children?: import("react").ReactNode;
  } & import("react").RefAttributes<HTMLDivElement>) => import("react").ReactElement<any, string | import("react").JSXElementConstructor<any>> | null;
  export type LocReactNode = JSX.Element | string;
  export enum ParagraphStyle {
  	None = 0,
  	Heading1 = 1,
  	Heading2 = 2,
  	Heading3 = 3,
  	Heading4 = 4,
  	Heading5 = 5,
  	Heading6 = 6,
  	ListItem = 7
  }
  export type FormattedTextRenderResult = [
  	node: ReactNode,
  	style: ParagraphStyle,
  	images: string[]
  ];
  export interface FormattedTextRenderer {
  	render(str: string): FormattedTextRenderResult;
  }
  export interface FormattedTextTheme {
  	p: string;
  	h1: string;
  	h2: string;
  	h3: string;
  	h4: string;
  	h5: string;
  	h6: string;
  	link: string;
  	listItem: string;
  }
  export interface FormattedTextProps extends HTMLAttributes<HTMLParagraphElement> {
  	focusKey?: FocusKey;
  	text?: LocReactNode;
  	theme?: Partial<FormattedTextTheme>;
  	renderer?: FormattedTextRenderer;
  	onLinkSelect?: (data: string) => void;
  }
  export export const FormattedText: ({ focusKey, text, theme: partialTheme, renderer, className, onLinkSelect, ...props }: FormattedTextProps) => JSX.Element;
  export interface FormattedParagraphsTheme extends FormattedTextTheme {
  	paragraphs: string;
  }
  export interface FormattedParagraphsProps extends HTMLAttributes<HTMLDivElement> {
  	focusKey?: FocusKey;
  	/** @deprecated */
  	text?: string | string[];
  	theme?: Partial<FormattedParagraphsTheme>;
  	renderer?: FormattedTextRenderer;
  	onLinkSelect?: (data: string) => void;
  	maxLineLength?: number;
  	splitLineLength?: number;
  }
  export export const FormattedParagraphs: ({ focusKey, text, theme: partialTheme, renderer, className, children, onLinkSelect, maxLineLength, splitLineLength, ...props }: PropsWithChildren<FormattedParagraphsProps>) => JSX.Element;
  export export class MarkdownRenderer implements FormattedTextRenderer {
  	render(str: string): FormattedTextRenderResult;
  }
  export type LinkRenderer = (data: string, text: ReactNode[], key?: string | number | null | undefined) => ReactNode | undefined;
  export export class MarkupRenderer implements FormattedTextRenderer {
  	private linkRenderer?;
  	constructor(linkRenderer?: LinkRenderer | undefined);
  	render(str: string): FormattedTextRenderResult;
  }
  // https://coherent-labs.com/Documentation/cpp-gameface/d1/dea/shape_morphing.html
  // https://coherent-labs.com/Documentation/cpp-gameface/d4/d08/interface_morph_animation.html
  export export interface HTMLImageElement {
  	getSrcSVGAnimation(): MorphAnimation | null;
  }
  export export interface Element {
  	getMaskSVGAnimation(): MorphAnimation | null;
  }
  export export interface MorphAnimation {
  	currentTime: number;
  	playbackRate: number;
  	play(): void;
  	pause(): void;
  	reverse(): void;
  	playFromTo(playTime: number, pauseTime: number, callback?: () => void): void;
  }
  
  export {
  	ButtonProps$1 as ButtonProps,
  	InfoRow as PanelSectionRow,
  	InfoSection as PanelSection,
  	InfoSectionFoldout as PanelFoldout,
  	PanelProps$1 as PanelProps,
  	UITriggeredConfirmationDialog as ConfirmationDialog,
  };
  
  export {};
  
}