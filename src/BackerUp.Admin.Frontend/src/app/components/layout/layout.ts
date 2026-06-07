import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavBar } from '../nav-bar/nav-bar';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, NavBar],
  templateUrl: './layout.html',
  styleUrl: './layout.scss',
})
export class Layout {}
