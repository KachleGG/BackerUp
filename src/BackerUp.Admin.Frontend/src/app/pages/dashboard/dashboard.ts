import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LogsService } from '../../services/logs.service';
import { Log } from '../../models/log.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  logs: Log[] = [];
  loadingLogs = true;

  constructor(private logsService: LogsService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.logsService.getAll().subscribe({
      next: logs => (this.logs = logs.slice(0, 6)),
      error: () => {
        this.logs = [];
        this.loadingLogs = false;
      },
      complete: () => (this.loadingLogs = false),
    });
  }
}
