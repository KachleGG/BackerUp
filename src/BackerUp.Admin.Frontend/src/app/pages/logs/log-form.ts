import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LogsService } from '../../services/logs.service';
import { LogLevel } from '../../models/log.model';

@Component({
  selector: 'app-log-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './log-form.html',
  styleUrl: './logs.scss',
})
export class LogForm {
  jobsClientsId: number | null = null;
  level: LogLevel = 'Info';
  description = '';

  levels: LogLevel[] = ['Info', 'Warning', 'Error'];

  constructor(private logsService: LogsService, private router: Router) {}

  save(): void {
    this.logsService.create({ jobsClientsId: this.jobsClientsId!, level: this.level, description: this.description })
      .subscribe(() => this.router.navigate(['/logs']));
  }

  cancel(): void {
    this.router.navigate(['/logs']);
  }
}
