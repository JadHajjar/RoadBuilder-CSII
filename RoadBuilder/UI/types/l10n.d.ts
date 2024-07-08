declare module "cs2/l10n" {
  import { FC, FunctionComponent, MemoExoticComponent } from 'react';
  
  export interface Typed<T extends string> {
  	__Type: T;
  }
  export type TypeFromMap<T extends Record<string, any>> = {
  	[K in keyof T]: K extends string ? T[K] & Typed<K> : never;
  }[keyof T];
  export export enum Unit {
  	Integer = "integer",
  	IntegerRounded = "integerRounded",
  	IntegerPerMonth = "integerPerMonth",
  	IntegerPerHour = "integerPerHour",
  	FloatSingleFraction = "floatSingleFraction",
  	FloatTwoFractions = "floatTwoFractions",
  	FloatThreeFractions = "floatThreeFractions",
  	Percentage = "percentage",
  	PercentageSingleFraction = "percentageSingleFraction",
  	Angle = "angle",
  	Length = "length",
  	Area = "area",
  	Volume = "volume",
  	VolumePerMonth = "volumePerMonth",
  	Weight = "weight",
  	WeightPerCell = "weightPerCell",
  	WeightPerMonth = "weightPerMonth",
  	Power = "power",
  	Energy = "energy",
  	DataRate = "dataRate",
  	DataBytes = "dataBytes",
  	DataMegabytes = "dataMegabytes",
  	Money = "money",
  	MoneyPerCell = "moneyPerCell",
  	MoneyPerMonth = "moneyPerMonth",
  	MoneyPerHour = "moneyPerHour",
  	MoneyPerDistance = "moneyPerDistance",
  	MoneyPerDistancePerMonth = "moneyPerDistancePerMonth",
  	BodiesPerMonth = "bodiesPerMonth",
  	XP = "xp",
  	Temperature = "temperature",
  	NetElevation = "netElevation",
  	ScreenFrequency = "screenFrequency",
  	Custom = "custom"
  }
  export enum LocElementType {
  	Bounds = "Game.UI.Localization.LocalizedBounds",
  	Fraction = "Game.UI.Localization.LocalizedFraction",
  	Number = "Game.UI.Localization.LocalizedNumber",
  	String = "Game.UI.Localization.LocalizedString"
  }
  export interface LocElements {
  	[LocElementType.Bounds]: LocalizedBounds;
  	[LocElementType.Fraction]: LocalizedFraction;
  	[LocElementType.Number]: LocalizedNumber;
  	[LocElementType.String]: LocalizedString;
  }
  export type LocElement = TypeFromMap<LocElements>;
  export interface LocalizedBounds {
  	min: number;
  	max: number;
  	unit?: Unit;
  }
  export interface LocalizedFraction {
  	value: number;
  	total: number;
  	unit?: Unit;
  }
  export interface LocalizedNumber {
  	value: number;
  	unit?: Unit;
  	signed: boolean;
  }
  export interface LocalizedString {
  	id: string | null;
  	value: string | null;
  	args: Record<string, LocElement> | null;
  }
  export interface UnitSettings {
  	timeFormat: TimeFormat;
  	temperatureUnit: TemperatureUnit;
  	unitSystem: UnitSystem;
  }
  export enum TimeFormat {
  	TwentyFourHours = 0,
  	TwelveHours = 1
  }
  export enum TemperatureUnit {
  	Celsius = 0,
  	Fahrenheit = 1,
  	Kelvin = 2
  }
  export enum UnitSystem {
  	Metric = 0,
  	Freedom = 1
  }
  export interface Localization {
  	translate(id: string, fallback?: string | null): string | null;
  	unitSettings: UnitSettings;
  }
  export function useCachedLocalization(): Localization;
  export interface LocComponent<P = unknown> extends MemoExoticComponent<FunctionComponent<P>> {
  	renderString: LocStringRenderer<P>;
  	propsAreEqual: PropsAreEqual<P>;
  }
  export type LocStringRenderer<P> = (loc: Localization, props: P) => string;
  export type PropsAreEqual<P> = (prevProps: P, nextProps: P) => boolean;
  export type LocReactNode = JSX.Element | string;
  export interface LocalizedProps {
  	value: LocElement;
  }
  export export const Localized: LocComponent<LocalizedProps>;
  export interface LocalizedBoundsProps {
  	min: number;
  	max: number;
  	unit?: Unit;
  }
  export const LocalizedBounds$1: LocComponent<LocalizedBoundsProps>;
  export interface SimulationDate {
  	year: number;
  	month: number;
  }
  export interface LocalizedDateProps {
  	value: SimulationDate;
  }
  export export const LocalizedDate: LocComponent<LocalizedDateProps>;
  export interface LocalizedDurationProps {
  	value: number;
  	daysPerYear: number;
  	maxMonths?: number;
  }
  export export const LocalizedDuration: LocComponent<LocalizedDurationProps>;
  export enum NameType {
  	Custom = "names.CustomName",
  	Localized = "names.LocalizedName",
  	Formatted = "names.FormattedName"
  }
  export type Name = CustomName | LocalizedName | FormattedName;
  export interface CustomName extends Typed<NameType.Custom> {
  	name: string;
  }
  export interface LocalizedName extends Typed<NameType.Localized> {
  	nameId: string;
  }
  export interface FormattedName extends Typed<NameType.Formatted> {
  	nameId: string;
  	nameArgs: {
  		[key: string]: string;
  	};
  }
  export interface LocalizedNameProps {
  	value: Name;
  }
  export export const LocalizedEntityName: FC<LocalizedNameProps>;
  export interface LocalizedFractionProps {
  	value: number;
  	total: number;
  	unit?: Unit;
  }
  export const LocalizedFraction$1: LocComponent<LocalizedFractionProps>;
  export interface LocalizedNumberProps {
  	value: number;
  	unit?: Unit;
  	signed?: boolean;
  }
  export const LocalizedNumber$1: LocComponent<LocalizedNumberProps>;
  export interface LocalizedPercentageProps {
  	value: number;
  	max: number;
  }
  export export const LocalizedPercentage: LocComponent<LocalizedPercentageProps>;
  export interface LocalizedStringProps {
  	id: string | null;
  	fallback?: string | null;
  	showIdOnFail?: boolean;
  	args?: Record<string, LocReactNode> | null;
  }
  export const LocalizedString$1: LocComponent<LocalizedStringProps>;
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
  	LocalizedBounds$1 as LocalizedBounds,
  	LocalizedFraction$1 as LocalizedFraction,
  	LocalizedNumber$1 as LocalizedNumber,
  	LocalizedString$1 as LocalizedString,
  	useCachedLocalization as useLocalization,
  };
  
  export {};
  
}