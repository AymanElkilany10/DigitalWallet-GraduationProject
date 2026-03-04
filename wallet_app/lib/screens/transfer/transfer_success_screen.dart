import 'package:flutter/material.dart';
import '../../theme.dart';

class TransferSuccessScreen extends StatelessWidget {
  final Map<String, dynamic> recipient;
  final double amount;
  final Map<String, dynamic> transactionData;

  const TransferSuccessScreen({
    super.key,
    required this.recipient,
    required this.amount,
    required this.transactionData,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                width: 110,
                height: 110,
                decoration: BoxDecoration(
                  color: AppColors.success.withOpacity(0.1),
                  shape: BoxShape.circle,
                ),
                child: const Icon(Icons.check_circle,
                    color: AppColors.success, size: 64),
              ),
              const SizedBox(height: 24),
              const Text('Transfer Successful!',
                  style: TextStyle(
                      fontSize: 24,
                      fontWeight: FontWeight.bold,
                      color: AppColors.textPrimary)),
              const SizedBox(height: 8),
              Text('You sent \$${amount.toStringAsFixed(2)} to',
                  style: const TextStyle(
                      color: AppColors.textSecondary, fontSize: 15)),
              const SizedBox(height: 6),
              Text(recipient['fullName'] ?? '',
                  style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.w600,
                      color: AppColors.primary)),
              const SizedBox(height: 32),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(18),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                  boxShadow: [
                    BoxShadow(
                        color: Colors.black.withOpacity(0.06),
                        blurRadius: 12,
                        offset: const Offset(0, 4))
                  ],
                ),
                child: Column(children: [
                  _receiptRow('Transaction ID',
                      transactionData['id']?.toString().substring(0, 8) ?? '—'),
                  _receiptRow('Amount', '\$${amount.toStringAsFixed(2)}'),
                  _receiptRow('Date', DateTime.now().toString().substring(0, 10)),
                  _receiptRow('Status', 'Completed'),
                ]),
              ),
              const SizedBox(height: 32),
              ElevatedButton(
                onPressed: () =>
                    Navigator.popUntil(context, (r) => r.isFirst),
                child: const Text('View Receipt'),
              ),
              const SizedBox(height: 12),
              TextButton(
                onPressed: () =>
                    Navigator.popUntil(context, (r) => r.isFirst),
                child: const Text('Back to Home'),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _receiptRow(String label, String value) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 7),
    child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label,
              style: const TextStyle(
                  color: AppColors.textSecondary, fontSize: 13)),
          Text(value,
              style: const TextStyle(
                  fontWeight: FontWeight.w600, fontSize: 13)),
        ]),
  );
}