import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LogsService } from '../../services/logs.service';
import { Log } from '../../models/log.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  logs: Log[] = [];

  constructor(private logsService: LogsService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.logsService.getAll().subscribe(data => (this.logs = data));
  }
}
