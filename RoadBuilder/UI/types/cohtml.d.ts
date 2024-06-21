declare module "cohtml/cohtml" {
  // There are minor edits here to make cohtml.d.ts valid with more strict tscofnig setup denoted with - // modified by Colossal - comment
  
  interface SingleArgumentCallback<T> {
    (result: T): any;
  }
  
  interface ArbitraryCallback {
    (...args: any[]): any; // modified by Colossal
  }
  
  export interface EventHandle {
    /**
    * Detach this handler from the event
    **/
    clear(): void;
  }
  
  interface VirtualList {
    /**
    * Index from which the data-bind-for will start generating DOM elements. The default value is 0.
    **/
    startIndex: number;
  
    /**
    * The maximum number of elements that will be generated from the data-bind-for.
    **/
    pageSize: number;
  }
  
  interface AttributeHandler {
    /**
    * This will be executed only once per element when the attribute attached to it is bound with a model.
    * Set up any initial state, event handlers, etc. here.
    * @param element The DOM element to which the handler is attached
    * @param value The result from the evaluation of the attribute's expression in the HTML
    **/
    init(element: Element, value: any): void;
  
    /**
    * This will be executed only once per element when the element is detached from the DOM.
    * Cleanup state, event handlers, etc. here.
    * @param element The DOM element to which the handler is attached
    **/
    deinit(element: Element): void;
  
    /**
    * This will be executed every time when the model on which the attribute is attached is synchronized.
    * @param element The DOM element to which the handler is attached
    * @param value The result from the evaluation of the attribute's expression in the HTML
    **/
    update(element: Element, value: any): void;
  }
  
  interface AttributeHandlerConstructor {
    new(): AttributeHandler;
  }
  
  interface Deferred<T> extends Promise<T> {
    /**
    * Resolve the promise with the specified value. All success handlers will be called with value
    * @param value The success value of the promise
    **/
    resolve(value: T): void;
  
    /**
    * Reject the promise with the specified value. All failure handlers will be called with value
    * @param value The failure value of the promise
    **/
    reject(value: T): void;
  }
  
  export interface Engine { // modified by Colossal
    /**
    * A promise that is resolved after the page has loaded and the bindings are ready
    **/
    whenReady: Promise<any>;
  
    /**
    * Register handler for an event
    * @param name The name of the event
    * @param callback A function to be executed when the event has been triggered
    * @param context *this* context for the function, by default the engine object
    * @return handle for unsubscribing this callback to the event
    **/
    on(name: string, callback: ArbitraryCallback, context?: any): EventHandle;
  
    /**
    * Remove handler for an event
    * @param name The name of the event, by default removes all callbacks
    * @param callback The callback to be removed, by default removes a callback regardless of the context
    * @param context *this* context for the function, when specified only the callback with the specified context will be removed
    **/
    off(name: string, callback?: ArbitraryCallback, context?: any): void;
  
    /**
    * Trigger an event
    * This function will trigger any C++ handler registered for this event with `Coherent::UI::View::RegisterForEvent`
    * @param name The name of the event
    * @param args Any extra arguments to be passed to the event handlers
    **/
    trigger(name: string, ...args: any[]): void;
  
    /**
    * Call asynchronously a C++ handler and retrieve the result
    * The C++ handler must have been registered with `Coherent::UI::View::BindCall`
    * @param name The name of the C++ handler to be called
    * @param args Any extra parameters to be passed to the C++ handler
    * @return A promise for the result of the C++ function
    **/
    call(name: string, ...args: any[]): Promise<any>;
  
    /**
    * Registers a JavaScript data binding model
    * @param name The name of the model
    * @param model The model's definition
    **/
    createJSModel(name: string, model: object): void;
  
    /**
    * Applies the changes accumulated by updateWholeModel to the corresponding JavaScript objects.
    **/
    synchronizeModels(): void;
  
    /**
    * Marks a model as dirty. Properties will be synchronized using the synchronizeModels call.
    * @param model The model to be marked as dirty
    **/
    updateWholeModel(model: object): void;
  
    /**
    * Unregisters a model and removes the global variable that is associated with it.
    * @param model The model to be removed
    **/
    unregisterModel(model: object): void;
  
    /**
    * Creates a virtual list object to be used for pagination in data-bind-for
    * @return VirtualList object for configuring the pagination options
    **/
    createVirtualList(): VirtualList;
  
    /**
    * Registers a custom handler for a given data-bind attribute name
    * @param attributeName The name for the custom data-bind attribute, excluding the "data-bind-" prefix
    * @param attributeHandler The AttributeHandler for the data-bind attribute
    **/
    registerBindingAttribute(attributeName: string, attributeHandler: AttributeHandlerConstructor): void;
  
    /**
    * Registers a JavaScript data binding model
    * @param id The id that will be requested in the localization manager
    * @return The translated text from the localization manager
    **/
    translate(id: string): string;
  
    /**
    * Updates all localized elements having `data-l10n-id`. Useful after changing the locale.
    **/
    reloadLocalization(): void;
  }
  
  export const engine: Engine; // modified by Colossal
  export default engine;
  
}