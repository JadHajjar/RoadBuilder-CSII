import { ModRegistrar } from "cs2/modding";
import { HelloWorldComponent } from "mods/hello-world";

const register: ModRegistrar = (moduleRegistry) => {

    moduleRegistry.append('Menu', HelloWorldComponent);
}

export default register;