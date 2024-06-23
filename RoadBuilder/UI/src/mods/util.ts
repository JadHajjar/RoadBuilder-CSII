export const classNames = (classes: {[className in string]: boolean}, ...baseClasses: string[]) => {
    let re = baseClasses.join(' ');
    re += Object.entries(classes)
        .filter(([className, value]) => value)
        .map(([className, value]) => className)
        .join(' ')
    return re;
}

export const range = (i: number, n: number) => ({
    [Symbol.iterator]: () => ({ 
        next: () => ({ done: i > n, value: i++  }) 
    })
});

export const MouseButtons = {
    Secondary: 2,
    Primary: 0
};