// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

/// @title Empathic Provenance Registry
/// @notice Stores compact evidence of cultural-work provenance. The creative content itself stays off-chain.
contract EmpathicProvenanceRegistry {
    struct Record {
        bytes32 contentHash;
        address registrant;
        string workId;
        uint64 anchoredAt;
    }

    mapping(bytes32 => Record) private records;

    event WorkAnchored(bytes32 indexed contentHash, address indexed registrant, string workId, uint64 anchoredAt);

    function anchor(bytes32 contentHash, string calldata workId) external {
        require(contentHash != bytes32(0), "Invalid hash");
        require(records[contentHash].anchoredAt == 0, "Already registered");

        uint64 timestamp = uint64(block.timestamp);
        records[contentHash] = Record(contentHash, msg.sender, workId, timestamp);
        emit WorkAnchored(contentHash, msg.sender, workId, timestamp);
    }

    function verify(bytes32 contentHash) external view returns (Record memory) {
        return records[contentHash];
    }
}
