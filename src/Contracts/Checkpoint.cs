namespace Contracts;

public record Checkpoint(
    string ModelKind,
    string TokenizerKind,
    object TokenizerPayload,
    object ModelPayload,
    int Seed,

    string ContractFingerprintChain
);