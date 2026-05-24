-- Use existing database (no CREATE DATABASE allowed)
USE 3b1_kachlikmarek_db1;

-- BackupJobs
CREATE TABLE BackupJobs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    method ENUM('Full', 'Incremental', 'Differential') NOT NULL,
    timing VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Clients
CREATE TABLE Clients (
    id CHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Users
CREATE TABLE Users (
    id CHAR(36) PRIMARY KEY,
    username VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Sources
CREATE TABLE Sources (
    id INT AUTO_INCREMENT PRIMARY KEY,
    job_id INT NOT NULL,
    path VARCHAR(500) NOT NULL,
    CONSTRAINT fk_sources_job
        FOREIGN KEY (job_id) REFERENCES BackupJobs(id)
        ON DELETE CASCADE
);

-- Targets
CREATE TABLE Targets (
    id INT AUTO_INCREMENT PRIMARY KEY,
    job_id INT NOT NULL,
    path VARCHAR(500) NOT NULL,
    CONSTRAINT fk_targets_job
        FOREIGN KEY (job_id) REFERENCES BackupJobs(id)
        ON DELETE CASCADE
);

-- Retention (1:1 with BackupJobs)
CREATE TABLE Retention (
    id INT AUTO_INCREMENT PRIMARY KEY,
    job_id INT NOT NULL UNIQUE,
    count INT NOT NULL DEFAULT 3,
    size INT NOT NULL DEFAULT 1,
    CONSTRAINT fk_retention_job
        FOREIGN KEY (job_id) REFERENCES BackupJobs(id)
        ON DELETE CASCADE
);

-- JobsClients (many-to-many)
CREATE TABLE JobsClients (
    id INT AUTO_INCREMENT PRIMARY KEY,
    job_id INT NOT NULL,
    client_id CHAR(36) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT fk_jobsclients_job
        FOREIGN KEY (job_id) REFERENCES BackupJobs(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_jobsclients_client
        FOREIGN KEY (client_id) REFERENCES Clients(id)
        ON DELETE CASCADE
);

-- Logs
CREATE TABLE Logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    jobs_clients_id INT NOT NULL,
    level ENUM('Info', 'Warning', 'Error') NOT NULL,
    description TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_logs_jobsclients
        FOREIGN KEY (jobs_clients_id) REFERENCES JobsClients(id)
        ON DELETE CASCADE
);