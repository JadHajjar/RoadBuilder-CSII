declare module "cs2/modding" {
  import { ComponentType } from 'react';
  
  export type ModuleRegistryExtend = <T extends ComponentType<any>>(curr: T) => (props: any) => JSX.Element;
  export type ModuleRegistryAppend = ComponentType<{}> | (() => JSX.Element);
  export type AppendHookTargets = "Menu" | "Editor" | "Game" | "GameTopLeft" | "GameTopRight" | "GameBottomRight";
  export type ModuleRegistry = {
  	get(modulePath: string, exportName: string): any;
  	add(modulePath: string, module: Record<string, any>): void;
  	override(modulePath: string, exportName: string, newValue: any): void;
  	extend(modulePath: string, exportNameOrSCSSValue: string | any, extendCb?: ModuleRegistryExtend): void;
  	append(modulePath: string, exportName: string, appendedComponent?: ModuleRegistryAppend, index?: number): void;
  	append(target: AppendHookTargets, appendedComponent: ModuleRegistryAppend, index?: number, _?: never): void;
  	registry: Map<string, Record<string, any>>;
  	find(query: string | RegExp): [
  		path: string,
  		...exports: string[]
  	][];
  	reset(): void;
  };
  export type ModRegistrar = (moduleRegistry: ModuleRegistry) => void;
  export export const findModule: (query: string | RegExp) => [
  	path: string,
  	...exports: string[]
  ][];
  export export const getModule: (modulePath: string, exportName: string) => any;
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
  
  export {};
  
}