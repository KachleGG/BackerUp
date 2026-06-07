import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UsersService } from '../../services/users.service';
import { User } from '../../models/models';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users implements OnInit {
  users: WritableSignal<User[]> = signal<User[]>([]);

  trackById(index: number, item: User) { return item.id; }

  constructor(private usersService: UsersService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.usersService.getAll().subscribe(data => this.users.set(data));
  }

  add(): void {
    this.router.navigate(['/users/add']);
  }

  edit(id: string): void {
    this.router.navigate(['/users/edit', id]);
  }

  delete(id: string): void {
    if (!confirm('Are you sure you want to delete this user?')) return;
    this.usersService.delete(id).subscribe(() => {
      // remove locally for immediate UI feedback
      this.users.update(list => list.filter(u => u.id !== id));
    });
  }
}

