import * as React from 'react';

declare module 'react' {
    interface HTMLAttributes<T> extends React.AriaAttributes, React.DOMAttributes<T> {
      // extends React's HTMLAttributes
      cohinline?: string;
    }
}