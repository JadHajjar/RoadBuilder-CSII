declare module "cs2/api" {
  export interface ValueBinding<T> {
  	readonly value: T;
  	subscribe(listener?: BindingListener<T>): ValueSubscription<T>;
  	dispose(): void;
  }
  export interface BindingListener<T> {
  	(value: T): void;
  }
  export interface Subscription {
  	dispose(): void;
  }
  export interface ValueSubscription<T> extends Subscription {
  	readonly value: T;
  	setChangeListener(listener: BindingListener<T>): void;
  }
  export export function bindValue<T>(group: string, name: string, fallbackValue?: T): ValueBinding<T>;
  export export function trigger(group: string, name: string, ...args: any[]): void;
  export export function call<T>(group: string, name: string, ...args: any[]): Promise<T>;
  /** Subscribe to a ValueBinding. Return fallback value or throw an error if the binding is not registered on the C# side */
  export export function useValue<V>(binding: ValueBinding<V>): V;
  
  export {};
  
}