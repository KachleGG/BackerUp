import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LogsService } from '../../services/logs.service';
import { Log } from '../../models/models';

@Component({
  selector: 'app-logs',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './logs.html',
  styleUrl: './logs.scss',
})
export class Logs implements OnInit {
  logs: Log[] = [];

  constructor(private logsService: LogsService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.logsService.getAll().subscribe(data => this.logs = data);
  }


}
