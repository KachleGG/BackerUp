import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-form.html',
  styleUrl: './users.scss',
})
export class UserForm implements OnInit {
  isEdit = false;
  id: string | null = null;

  username = '';
  password = '';

  constructor(
    private usersService: UsersService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    this.isEdit = !!this.id;

    if (this.isEdit && this.id) {
      this.usersService.getById(this.id).subscribe(u => {
        this.username = u.username;
        this.password = '';
        this.cdr.detectChanges();
      });
      return;
    }

    this.username = '';
    this.password = '';
    this.cdr.detectChanges();
  }

  save(): void {
    if (this.isEdit && this.id) {
      const request: { username: string; password?: string } = { username: this.username };
      if (this.password) request.password = this.password;
      this.usersService.update(this.id, request).subscribe(() => this.router.navigate(['/users']));
    } else {
      this.usersService.create({ username: this.username, password: this.password }).subscribe(() => this.router.navigate(['/users']));
    }
  }

  cancel(): void {
    this.router.navigate(['/users']);
  }
}
