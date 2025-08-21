import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, NavLink, Routes, Route } from "react-router";
import Home from './Home';
import BlazorComponent from './BlazorComponent';

const basename = location.hostname == "localhost" ? '' : '/blazor-wasm-react';

ReactDOM.createRoot(document.querySelector('#root')).render(
   <BrowserRouter basename={basename}>
      <div class="container">
         <nav>
            <NavLink to="/" end>Home</NavLink>
            <NavLink to="/counter" end>Counter</NavLink>
            <NavLink to="/counter-10" end>Counter 10</NavLink>
            <NavLink to="/date-time-now">Date Time Now</NavLink>
         </nav>
         <main>
            <Routes>
               <Route path="/" element={<Home />} />
               <Route path="/counter" element={<BlazorComponent component="blazor-counter" />} />
               <Route path="/counter-10" element={<BlazorComponent component="blazor-counter" initial="10" />} />
               <Route path="/date-time-now" element={<BlazorComponent component="blazor-date-time-now" />} />
            </Routes>
         </main>
      </div>
   </BrowserRouter>
);
