import { Component, OnInit, signal, WritableSignal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ClientsService } from '../../services/clients.service';
import { Client } from '../../models/client.model';

@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './clients.html',
  styleUrl: './clients.scss',
})
export class Clients implements OnInit {
  clients: WritableSignal<Client[]> = signal<Client[]>([]);
  viewingPending = false;

  trackById(index: number, item: Client) { return item.id; }

  constructor(private clientsService: ClientsService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.clientsService.getAll().subscribe(data => this.clients.set(data));
  }

  loadPending(): void {
    this.clientsService.getPending().subscribe(data => this.clients.set(data));
  }

  togglePending(): void {
    this.viewingPending = !this.viewingPending;
    if (this.viewingPending) this.loadPending();
    else this.load();
  }

  add(): void {
    this.router.navigate(['/clients/add']);
  }

  edit(id: string): void {
    this.router.navigate(['/clients/edit', id]);
  }

  delete(id: string): void {
    if (!confirm('Are you sure you want to delete this client?')) return;
    this.clientsService.delete(id).subscribe(() => this.clients.update(list => list.filter(c => c.id !== id)));
  }

  approve(id: string): void {
    if (!confirm('Approve this client?')) return;
    this.clientsService.approve(id).subscribe(() => this.clients.update(list => list.filter(c => c.id !== id)));
  }
}

