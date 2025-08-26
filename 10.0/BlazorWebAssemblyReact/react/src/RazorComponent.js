import React from 'react';
import { start } from 'blazor';

export default function RazorComponent({ component, ...params }) {
    const [loading, setLoaded] = React.useState(true);
    React.useEffect(() => {
        start().then(setLoaded);
    }, []);

    return (
        <div>
            {loading ? (
                <span>Loading...</span>
            ) : (
                React.createElement(component, { ...params })
            )}
        </div>
    );
}
