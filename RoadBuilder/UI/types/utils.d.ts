declare module "cs2/utils" {
  export type EqualityComparer<T> = (a: T, b: T) => boolean;
  export interface Entity {
  	index: number;
  	version: number;
  }
  export export function entityKey({ index, version }: Entity): string;
  export export function parseEntityKey(value: any): Entity | undefined;
  export export function entityEquals(a: Entity | null | undefined, b: Entity | null | undefined): boolean;
  export export function isNullOrEmpty(s: string | null | undefined): boolean;
  /**
   * Performs equality by iterating through keys on an object and returning false
   * when any key has values which are not strictly equal between the arguments.
   * Returns true when the values of all keys are strictly equal.
   */
  export export function shallowEqual(a: any, b: any, depth?: number): boolean;
  export export function useMemoizedValue<T>(value: T, equalityComparer: EqualityComparer<T>): T;
  export export function formatLargeNumber(value: number): string;
  export export function useFormattedLargeNumber(value: number): string;
  export export function useRem(): number;
  export export function useCssLength(length: string): number;
  
  export {};
  
}