﻿using System;

namespace Foundatio.Queues {
    public class QueueEntry<T> : IDisposable where T: class {
        private readonly IQueue<T> _queue;
        private bool _isCompleted;

        public QueueEntry(string id, T value, IQueue<T> queue, DateTime enqueuedTimeUtc, int attempts) {
            Id = id;
            Value = value;
            _queue = queue;
            EnqueuedTimeUtc = enqueuedTimeUtc;
            Attempts = attempts;
            DequeuedTimeUtc = DateTime.UtcNow;
        }

        public string Id { get; }
        public T Value { get; private set; }
        public DateTime EnqueuedTimeUtc { get; }
        public DateTime DequeuedTimeUtc { get; }
        public int Attempts { get; set; }

        public async Task CompleteAsync() {
            if (_isCompleted)
                return;

            _isCompleted = true;
            await _queue.CompleteAsync(this).AnyContext();
        }

        public async Task AbandonAsync() {
            await _queue.AbandonAsync(this).AnyContext();
        }

        public virtual void Dispose() {
            if (!_isCompleted)
                AbandonAsync().AnyContext().GetAwaiter().GetResult();
        }

        public QueueEntryMetadata ToMetadata() {
            return new QueueEntryMetadata
            {
                Id = Id,
                EnqueuedTimeUtc = EnqueuedTimeUtc,
                DequeuedTimeUtc = DequeuedTimeUtc,
                Attempts = Attempts
            };
        }
}