// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

/// @notice Phase-3 reference contract only. Do not deploy as production financial infrastructure without legal/security review.
contract FutureRevenueSplitter {
    struct Share { address payable account; uint16 basisPoints; }
    Share[] public shares;

    constructor(address payable[] memory accounts, uint16[] memory basisPoints) {
        require(accounts.length == basisPoints.length && accounts.length > 0, "Invalid shares");
        uint256 total;
        for (uint256 i; i < accounts.length; i++) {
            require(accounts[i] != address(0), "Zero address");
            total += basisPoints[i];
            shares.push(Share(accounts[i], basisPoints[i]));
        }
        require(total == 10_000, "Shares must total 100%");
    }

    receive() external payable {
        uint256 amount = msg.value;
        for (uint256 i; i < shares.length; i++) {
            (bool ok,) = shares[i].account.call{value: amount * shares[i].basisPoints / 10_000}("");
            require(ok, "Payment failed");
        }
    }
}
